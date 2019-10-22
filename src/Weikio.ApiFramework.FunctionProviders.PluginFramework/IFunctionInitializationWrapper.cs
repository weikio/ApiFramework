using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.FunctionProviders.PluginFramework
{
    public interface IFunctionInitializationWrapper
    {
        Func<Endpoint, Task<IEnumerable<Type>>> Wrap(List<MethodInfo> initializerMethods);
    }
}
