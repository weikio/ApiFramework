using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class ApiCatalogForTesting : IApiCatalog
    {
        private readonly List<ApiDefinition> _apis;

        public ApiCatalogForTesting(params ApiDefinition[] defs)
        {
            if (defs?.Any() == true)
            {
                _apis = new List<ApiDefinition>();

                foreach (var apiDefinition in defs)
                {
                    _apis.Add(apiDefinition);
                }
            }
        }

        public ApiCatalogForTesting(List<ApiDefinition> apis)
        {
            _apis = apis;
        }

        public Task Initialize(CancellationToken cancellationToken)
        {
            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; set; } = false;

        public List<ApiDefinition> List()
        {
            return _apis;
        }

        public Api Get(ApiDefinition definition)
        {
            var def = _apis.FirstOrDefault(x => x == definition);

            if (def == null)
            {
                return null;
            }

            var api = new Api(def, new List<Type>(), null);

            return api;
        }
    }
}
