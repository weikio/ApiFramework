using System.Reflection;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Apis
{
    internal class DefaultApiByAssemblyProvider : IApiByAssemblyProvider
    {
        private readonly IApiProvider _apiProvider;

        public DefaultApiByAssemblyProvider(IApiProvider apiProvider)
        {
            _apiProvider = apiProvider;
        }

        public ApiDefinition GetApiByAssembly(Assembly assembly)
        {
            var apis = _apiProvider.List();

            foreach (var apiDefinition in apis)
            {
                var api = _apiProvider.Get(apiDefinition);

                if (api.Assembly == assembly)
                {
                    return apiDefinition;
                }
            }

            return null;
        }
    }
}
