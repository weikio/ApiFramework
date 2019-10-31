using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public class ApiInitializationWrapper : IApiInitializationWrapper
    {
        private readonly ILogger<ApiInitializationWrapper> _logger;

        public ApiInitializationWrapper(ILogger<ApiInitializationWrapper> logger)
        {
            _logger = logger;
        }

        public Func<Endpoint, Task<IEnumerable<Type>>> Wrap(List<MethodInfo> initializerMethods)
        {
            if (initializerMethods?.Any() != true)
            {
                return null;
            }

            // This isn't performance sensitive code as it is usually called only once, when the endpoint is initialized. But try to minimize the dependencies
            // and remove Dynamitey as it isn't actually needed.
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

                IDictionary<string, object> configurationDictionary;

                if (endpoint.Configuration is IDictionary<string, object> configuration)
                {
                    configurationDictionary = configuration;
                }
                else if (endpoint.Configuration != null)
                {
                    configurationDictionary =
                        new Dictionary<string, object>(
                            JsonSerializer.Deserialize<Dictionary<string, object>>(
                                JsonSerializer.Serialize(endpoint.Configuration)), StringComparer.InvariantCultureIgnoreCase);
                }
                else
                {
                    configurationDictionary = new Dictionary<string, object>();
                }

                foreach (var initializerMethod in initializerMethods)
                {
                    var methodParameters = initializerMethod.GetParameters().ToList();
                    var arguments = new List<object>();

                    foreach (var methodParameter in methodParameters)
                    {
                        if (string.Equals(methodParameter.Name, "endpointroute", StringComparison.InvariantCultureIgnoreCase) &&
                            methodParameter.ParameterType == typeof(string))
                        {
                            arguments.Add(endpoint.Route);

                            continue;
                        };
                        
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

                    try
                    {
                        // Todo: Retry policy should be configurable
                        var initializationCount = 0;

                        var retryPolicy = Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(5), (exception, span, arg3, arg4) =>
                            {
                                endpoint.Status.UpdateStatus(EndpointStatusEnum.InitializingFailed,
                                    "Failed to initialize. Trying again shortly. Error: " + exception);
                                _logger.LogInformation($"Failed to initialize endpoint with {endpoint.Route}, trying again in {span.TotalSeconds} .");
                            });

                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            initializationCount += 1;

                            if (initializationCount > 1)
                            {
                                endpoint.Status.UpdateStatus(EndpointStatusEnum.Initializing, $"Initializing, attempt #{initializationCount}");
                            }

                            var tOut = (Task<IEnumerable<Type>>) initializerMethod.Invoke(null, arguments.ToArray());
                            var createdApis = await tOut;

                            result.AddRange(createdApis);
                        });
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
    }
}
