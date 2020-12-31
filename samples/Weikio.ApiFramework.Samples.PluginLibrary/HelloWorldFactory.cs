using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Samples.PluginLibrary
{
    public static class ApiFactory
    {
        public static Task<IEnumerable<Type>> Create()
        {
            var result = new List<Type>(){typeof(HelloWorldApi)};

            return Task.FromResult<IEnumerable<Type>>(result);
        }
    }
}
