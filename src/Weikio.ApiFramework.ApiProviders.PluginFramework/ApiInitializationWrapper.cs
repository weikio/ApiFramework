using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Context;
using Weikio.TypeGenerator;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    internal class ApiInitializationWrapper : IApiInitializationWrapper
    {
        private readonly ILogger<ApiInitializationWrapper> _logger;
        private readonly IOptionsMonitor<EndpointInitializationOptions> _endpointInitializationOptions;
        private readonly IServiceProvider _serviceProvider;

        public ApiInitializationWrapper(ILogger<ApiInitializationWrapper> logger, IOptionsMonitor<EndpointInitializationOptions> endpointInitializationOptions,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _endpointInitializationOptions = endpointInitializationOptions;
            _serviceProvider = serviceProvider;
        }

        public Func<Endpoint, Task<IEnumerable<Type>>> Wrap(List<Plugin> factoryTypes)
        {
            if (factoryTypes?.Any() != true)
            {
                return null;
            }

            // This isn't performance sensitive code as it is usually called only once, when the endpoint is initialized. 
            // Currently it seems that the best way to pass the parameters to the initializer is serializing and deserializing them through System.Text.Json.
            // As the assemblies are loaded into their own AssemblyLoadContext, we can't directly pass any complex type as the compiler thinks they are different.
            async Task<IEnumerable<Type>> InitializerFunc(Endpoint endpoint)
            {
                if (endpoint == null)
                {
                    throw new ArgumentNullException(nameof(endpoint));
                }

                _logger.LogDebug("Initializing {Endpoint} with {Route}", endpoint, endpoint.Route);

                var result = new List<Type>();

                IDictionary<string, object> configurationDictionary = null;

                if (endpoint.Configuration is IDictionary<string, object> configuration)
                {
                    configurationDictionary = configuration;
                }
                else if (endpoint.Configuration == null)
                {
                    configurationDictionary = new Dictionary<string, object>();
                }

                foreach (var factoryType in factoryTypes)
                {
                    var pluginAssemblyLoadContext = GetPluginAssemblyContext(factoryType);

                    if (pluginAssemblyLoadContext != null)
                    {
                        // Try to make sure that if the plugin uses CodeToAssemblyGenerator that it uses the same AssemblyLoadContext as the plugin
                        // Alternatively the ApiFactory can get context if it takes ApiEndpointFactoryContext as a constructor parameter
                        CodeToAssemblyGenerator.DefaultAssemblyLoadContextFactory = () => pluginAssemblyLoadContext;
                    }
                    else
                    {
                        CodeToAssemblyGenerator.DefaultAssemblyLoadContextFactory = () => new CustomAssemblyLoadContext(null);
                    }

                    var staticFactoryMethod = factoryType.Type
                        .GetMethods().FirstOrDefault(m => m.IsStatic && typeof(Task<IEnumerable<Type>>).IsAssignableFrom(m.ReturnType));

                    var hasStaticFactoryMethod = staticFactoryMethod != null && factoryType.Type.IsAbstract && factoryType.Type.IsSealed;

                    MethodInfo initializerMethod = null;
                    object factoryInstance = null;

                    if (hasStaticFactoryMethod)
                    {
                        initializerMethod = staticFactoryMethod;
                    }
                    else
                    {
                        var context = CreateContext(endpoint, factoryType);

                        // ReSharper disable once PossibleMistakenCallToGetType.2
                        var constructors = factoryType.Type.GetConstructors();

                        var takesContext = false;

                        foreach (var constructor in constructors)
                        {
                            if (constructor.GetParameters().Any(x => x.ParameterType == typeof(ApiEndpointFactoryContext)))
                            {
                                takesContext = true;

                                break;
                            }
                        }

                        if (takesContext)
                        {
                            factoryInstance = ActivatorUtilities.CreateInstance(_serviceProvider, factoryType, new object[] { context });
                        }
                        else
                        {
                            factoryInstance = ActivatorUtilities.CreateInstance(_serviceProvider, factoryType);
                        }

                        var allMethods = factoryInstance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                        foreach (var methodInfo in allMethods)
                        {
                            var methodReturnType = methodInfo.ReturnType;

                            // Not the prettiest solution but is OK for the first release
                            if (methodReturnType == typeof(Type) || methodReturnType == typeof(Task<Type>) ||
                                typeof(IEnumerable<Type>).IsAssignableFrom(methodReturnType) ||
                                typeof(Task<IEnumerable<Type>>).IsAssignableFrom(methodReturnType) ||
                                typeof(Task<List<Type>>).IsAssignableFrom(methodReturnType))
                            {
                                initializerMethod = methodInfo;

                                break;
                            }
                        }
                    }

                    if (initializerMethod == null)
                    {
                        throw new Exception("Couldn't find factory method from type");
                    }

                    var methodParameters = initializerMethod.GetParameters().ToList();
                    var arguments = new List<object>();

                    foreach (var methodParameter in methodParameters)
                    {
                        if (string.Equals(methodParameter.Name, "endpointroute", StringComparison.InvariantCultureIgnoreCase) &&
                            methodParameter.ParameterType == typeof(string))
                        {
                            arguments.Add(endpoint.Route);

                            continue;
                        }

                        if (string.Equals(methodParameter.Name, "configuration", StringComparison.InvariantCultureIgnoreCase) &&
                            methodParameter.ParameterType != typeof(string) && endpoint.Configuration != null)
                        {
                            if (configurationDictionary == null)
                            {
                                arguments.Add(endpoint.Configuration);

                                continue;
                            }

                            var jsonConfiguration = JsonConvert.SerializeObject(configurationDictionary);
                            var arg = JsonConvert.DeserializeObject(jsonConfiguration, methodParameter.ParameterType);
                            arguments.Add(arg);

                            continue;
                        }

                        if (configurationDictionary != null)
                        {
                            if (!configurationDictionary.ContainsKey(methodParameter.Name))
                            {
                                arguments.Add(GetDefaultValue(methodParameter.ParameterType));

                                continue;
                            }

                            var configurationValue = configurationDictionary[methodParameter.Name];

                            if (configurationValue is string)
                            {
                                var configurationValueAsMethodParameterType = Convert.ChangeType(configurationValue, methodParameter.ParameterType);
                                arguments.Add(configurationValueAsMethodParameterType);

                                continue;
                            }

                            var json = JsonSerializer.Serialize(configurationValue);

                            var obj = JsonSerializer.Deserialize(json, methodParameter.ParameterType,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            arguments.Add(obj);
                        }
                    }

                    try
                    {
                        var initializationCount = 0;

                        var endpointRetryPolicyOptions = _endpointInitializationOptions.Get(endpoint.Name);

                        var retryPolicy = endpointRetryPolicyOptions.RetryPolicy(endpointRetryPolicyOptions, endpoint, _logger);

                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            initializationCount += 1;

                            await endpointRetryPolicyOptions.OnInitialization(endpointRetryPolicyOptions, endpoint, initializationCount, _logger);

                            List<Type> createdApis = null;

                            if (factoryInstance == null)
                            {
                                var tOut = (Task<IEnumerable<Type>>) initializerMethod.Invoke(null, arguments.ToArray());
                                createdApis = (await tOut).ToList();
                            }
                            else
                            {
                                // Not the prettiest solution but is OK for the first release
                                var methodReturnType = initializerMethod.ReturnType;

                                if (methodReturnType == typeof(Type))
                                {
                                    var tOut = (Type) initializerMethod.Invoke(factoryInstance, arguments.ToArray());
                                    createdApis = new List<Type>() { tOut };
                                }
                                else if (methodReturnType == typeof(Task<Type>))
                                {
                                    var tOut = (Task<Type>) initializerMethod.Invoke(factoryInstance, arguments.ToArray());
                                    createdApis = new List<Type>() { (await tOut) };
                                }
                                else if (typeof(IEnumerable<Type>).IsAssignableFrom(methodReturnType))
                                {
                                    var tOut = (IEnumerable<Type>) initializerMethod.Invoke(factoryInstance, arguments.ToArray());
                                    createdApis = new List<Type>(tOut);
                                }
                                else if (typeof(Task<IEnumerable<Type>>).IsAssignableFrom(methodReturnType))
                                {
                                    var tOut = (Task<IEnumerable<Type>>) initializerMethod.Invoke(factoryInstance, arguments.ToArray());
                                    createdApis = new List<Type>((await tOut));
                                }
                                else if (typeof(Task<List<Type>>).IsAssignableFrom(methodReturnType))
                                {
                                    var tOut = (Task<List<Type>>) initializerMethod.Invoke(factoryInstance, arguments.ToArray());
                                    createdApis = new List<Type>((await tOut));
                                }
                                else
                                {
                                    throw new Exception("Unknown factory type");
                                }
                            }

                            foreach (var createdApi in createdApis)
                            {
                                if (typeof(IEndpointMetadataExtender).IsAssignableFrom(createdApi))
                                {
                                    var apiInstance = (IEndpointMetadataExtender) ActivatorUtilities.CreateInstance(_serviceProvider, createdApi);
                                    var endpointMetadata = await apiInstance.GetMetadata(endpoint);

                                    foreach (var metadata in endpointMetadata)
                                    {
                                        endpoint.AddMetadata(metadata);
                                    }
                                }
                            }

                            result.AddRange(createdApis);
                        });

                        await endpointRetryPolicyOptions.OnInitialized(endpointRetryPolicyOptions, endpoint, _logger);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to initialize {Endpoint} with {Route}", endpoint, endpoint.Route);

                        throw;
                    }
                }

                return result;
            }

            return InitializerFunc;
        }

        //https://stackoverflow.com/a/2490274/66988
        private object GetDefaultValue(Type t)
        {
            if (!t.IsValueType)
            {
                return null;
            }

            return Activator.CreateInstance(t);
        }

        private ApiEndpointFactoryContext CreateContext(Endpoint endpoint, Plugin plugin)
        {
            var result = new PluginFrameworkApiEndpointFactoryContext();
            result.Plugin = plugin;
            result.Endpoint = endpoint;
            result.AssemblyLoadContext = GetPluginAssemblyContext(plugin);
            
            return result;
        }

        private PluginAssemblyLoadContext GetPluginAssemblyContext(Plugin plugin)
        {
            if (plugin.Source is TypePluginCatalog typePluginCatalog)
            {
                return typePluginCatalog.Options.TypeFindingContext as PluginAssemblyLoadContext;
            }

            return null;
        }
    }
}
