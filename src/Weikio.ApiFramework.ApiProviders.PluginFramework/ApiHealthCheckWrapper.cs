using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    // TODO Make sure this is a smart solution.
    // This breaks the ability to pass in ILogger. Also not sure if we should support multiple health checks && health check methods (meaning health check which don't depend on Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions
    public class ApiHealthCheckWrapper : IApiHealthCheckWrapper
    {
        private readonly ILogger<ApiHealthCheckWrapper> _logger;

        public ApiHealthCheckWrapper(ILogger<ApiHealthCheckWrapper> logger)
        {
            _logger = logger;
        }

        public Func<Endpoint, Task<IHealthCheck>> Wrap(MethodInfo healthCheckFactoryMethod)
        {
            if (healthCheckFactoryMethod == null)
            {
                return null;
            }

            async Task<IHealthCheck> InitializeFunc(Endpoint endpoint)
            {
                _logger.LogDebug("Initializing {Endpoint} {HealthCheck}", endpoint, healthCheckFactoryMethod);

                IHealthCheck result;
                Dictionary<string, object> configurationDictionary;

                if (endpoint.Configuration is Dictionary<string, object> configurationDict)
                {
                    configurationDictionary = configurationDict;
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

                var methodParameters = healthCheckFactoryMethod.GetParameters().ToList();
                var arguments = new List<object>();

                foreach (var methodParameter in methodParameters)
                {
                    if (!configurationDictionary.ContainsKey(methodParameter.Name))
                    {
                        arguments.Add(GetDefaultValue(methodParameter.ParameterType));

                        continue;
                    }

                    var configurationValue = configurationDictionary[methodParameter.Name];

                    var json = JsonSerializer.Serialize(configurationValue);

                    var obj = JsonSerializer.Deserialize(json, methodParameter.ParameterType,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    arguments.Add(obj);
                }

                try
                {
                    var tOut = (Task<IHealthCheck>) healthCheckFactoryMethod.Invoke(null, arguments.ToArray());
                    result = await tOut;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to initialize {Endpoint} with {HealthCheck}", endpoint, healthCheckFactoryMethod);

                    throw;
                }

                return result;
            }

            return InitializeFunc;
        }

        public Func<Endpoint, HealthCheckContext, CancellationToken, Task<HealthCheckResult>> Wrap(Type healthCheckType)
        {
            if (healthCheckType == null)
            {
                return null;
            }

            var takesParams = true;
            var constructor = healthCheckType.GetConstructor(new[] { typeof(Endpoint) });

            if (constructor == null)
            {
                constructor = healthCheckType.GetConstructor(Type.EmptyTypes);

                if (constructor != null)
                {
                    takesParams = false;
                }
            }

            if (constructor == null)
            {
                throw new Exception($"Couldn't initialize health check from {healthCheckType.FullName}");
            }

            async Task<HealthCheckResult> HealthCheckFunc(Endpoint endpoint, HealthCheckContext context, CancellationToken cancellationToken)
            {
                if (endpoint == null)
                {
                    throw new ArgumentNullException(nameof(endpoint));
                }

                var healthCheckInstance = takesParams ? constructor.Invoke(new object[] { endpoint }) : constructor.Invoke(null);

                var healthCheckMethod = healthCheckInstance.GetType().GetMethod("CheckHealthAsync");

                if (healthCheckMethod == null)
                {
                    throw new Exception($"Failed to get CheckHealthAsync from {healthCheckInstance.GetType().FullName}");
                }

                var methodTask = (Task<HealthCheckResult>) healthCheckMethod.Invoke(healthCheckInstance, new object[] { context, cancellationToken });
                var result = await methodTask;

                return result;
            }

            return HealthCheckFunc;
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
