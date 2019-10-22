using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Abstractions
{
    public class Endpoint
    {
        public string Route { get; }
        public Function Function { get; }
        public object Configuration { get; private set; }
        public Func<Endpoint, Task<IHealthCheck>> HealthCheckFactory { get; }
        public List<Type> FunctionTypes { get; private set; }

        public IHealthCheck HealthCheck { get; private set; }

        private Dictionary<string, ResponseCacheConfiguration> _responseCacheConfigurations =
            new Dictionary<string, ResponseCacheConfiguration>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<KeyValuePair<string, ResponseCacheConfiguration>> ResponseCacheConfigurations => _responseCacheConfigurations;
        public EndpointStatus Status { get; }

        public bool HasHealthCheck
        {
            get
            {
                return HealthCheck != null;
            }
        }

        public Endpoint(string route, Function function, object configuration = null, Func<Endpoint, Task<IHealthCheck>> healthCheckFactory = null)
        {
            Route = route;
            Function = function;
            Configuration = configuration;
            HealthCheckFactory = healthCheckFactory;

            FunctionTypes = new List<Type>();
            Status = new EndpointStatus();
        }

        public async Task Initialize()
        {
            Status.UpdateStatus(EndpointStatusEnum.Initializing, "Initializing");

            try
            {
                var dynamicFunctions = await InitializeFunction();
                FunctionTypes.AddRange(dynamicFunctions);

                FunctionTypes.AddRange(Function.FunctionTypes);

                if (HealthCheckFactory != null)
                {
                    HealthCheck = await HealthCheckFactory(this);
                }

                Status.UpdateStatus(EndpointStatusEnum.Ready, "Ready");
            }
            catch (Exception e)
            {
                Status.UpdateStatus(EndpointStatusEnum.Failed, "Failed: " + e);
            }

//            if (moduleConfigSection?.GetChildren()?.Any() == true)
//            {
//                _responseCacheConfigurations = GetResponseCacheConfigurations(moduleConfigSection.GetSection("ResponseCache"));
//            }
//            else
//            {
//                _responseCacheConfigurations = new Dictionary<string, ResponseCacheConfiguration>();
//            }
        }

        private async Task<List<Type>> InitializeFunction()
        {
            var result = new List<Type>();

            if (Function.Initializer == null)
            {
                return result;
            }

            var task = Function.Initializer(this); //.Invoke(null, arguments.ToArray());
            var createdFunctions = await task;

            result.AddRange(createdFunctions);

//            foreach (var factoryType in Function.FactoryTypes)
//            {
//                var functionFactoryMethod = FindFunctionFactoryMethod(factoryType);
//
//                var parameters = functionFactoryMethod.GetParameters();
//                var arguments = new List<object>();
//
//                foreach (var p in parameters)
//                {
//                    var argument = Configuration;
//
//                    arguments.Add(argument);
//                }
//
//                var task = (Task<IEnumerable<Type>>) functionFactoryMethod.Invoke(null, arguments.ToArray());
//                var createdFunctions = await task;
//
//                result.AddRange(createdFunctions);
//            }

            return result;
        }

        private MethodInfo FindFunctionFactoryMethod(Type factoryType)
        {
            var methods = factoryType.GetMethods().ToList();

            var result = methods
                .FirstOrDefault(m => m.IsStatic && typeof(Task<IEnumerable<Type>>).IsAssignableFrom(m.ReturnType));

            return result;
        }

//
//        private static Dictionary<string, ResponseCacheConfiguration> GetResponseCacheConfigurations(IConfigurationSection cachingConfigSection)
//        {
//            var cacheConfigs = new Dictionary<string, ResponseCacheConfiguration>(StringComparer.OrdinalIgnoreCase);
//
//            if (cachingConfigSection == null)
//            {
//                return cacheConfigs;
//            }
//
//            foreach (var configSection in cachingConfigSection.GetChildren())
//            {
//                var resourcePath = configSection.Key;
//                var maxAge = configSection.GetValue<TimeSpan>("MaxAge");
//                var vary = configSection.GetSection("Vary")?.Get<string[]>() ?? new string[] { };
//
//                cacheConfigs.Add(resourcePath, new ResponseCacheConfiguration(resourcePath, maxAge, vary));
//            }
//
//            return cacheConfigs;
//        }
    }
}
