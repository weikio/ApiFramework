using System;
using System.Reflection;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.SDK
{
    public interface IApiByAssemblyProvider
    {
        ApiDefinition GetApiByAssembly(Assembly assembly);
        ApiDefinition GetApiByType(Type apiType);
    }
}
