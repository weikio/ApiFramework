using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public class ApiInitializationWrapper : IApiInitializationWrapper
    {
        private readonly ILogger<ApiInitializationWrapper> _logger;
        private readonly IOptionsMonitor<EndpointInitializationOptions> _endpointInitializationOptions;

        public ApiInitializationWrapper(ILogger<ApiInitializationWrapper> logger, IOptionsMonitor<EndpointInitializationOptions> endpointInitializationOptions)
        {
            _logger = logger;
            _endpointInitializationOptions = endpointInitializationOptions;
        }

        public Func<Endpoint, Task<IEnumerable<Type>>> Wrap(List<MethodInfo> initializerMethods)
        {
            if (initializerMethods?.Any() != true)
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
                        
                        if (string.Equals(methodParameter.Name, "configuration", StringComparison.InvariantCultureIgnoreCase) &&
                            methodParameter.ParameterType != typeof(string))
                        {
                            var arg = JsonSerializer.Deserialize(JsonSerializer.Serialize(configurationDictionary), methodParameter.ParameterType);
                            arguments.Add(arg);

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
                        var initializationCount = 0;
                        
                        var endpointRetryPolicyOptions = _endpointInitializationOptions.Get(endpoint.Name);

                        var retryPolicy = endpointRetryPolicyOptions.RetryPolicy(endpointRetryPolicyOptions, endpoint, _logger);

                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            initializationCount += 1;

                            await endpointRetryPolicyOptions.OnInitialization(endpointRetryPolicyOptions, endpoint, initializationCount, _logger);

                            var tOut = (Task<IEnumerable<Type>>) initializerMethod.Invoke(null, arguments.ToArray());
                            var createdApis = await tOut;

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
    }
}
