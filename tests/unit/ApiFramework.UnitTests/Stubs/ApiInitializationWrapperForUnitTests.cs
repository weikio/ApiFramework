using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.PluginFramework.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class ApiInitializationWrapperForUnitTests : IApiInitializationWrapper
    {
        public Func<Endpoint, Task<IEnumerable<Type>>> Wrap(List<Plugin> initializerMethods)
        {
            return null;
        }
    }
}
