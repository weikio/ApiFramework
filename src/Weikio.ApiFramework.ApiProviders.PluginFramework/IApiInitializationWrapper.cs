using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public interface IApiInitializationWrapper
    {
        Func<Endpoint, Task<IEnumerable<Type>>> Wrap(List<Type> factoryTypes);
    }
}
