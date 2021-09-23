using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Apis
{
    public class CompositeApiCatalog : List<IApiCatalog>, IApiCatalog
    {
        public CompositeApiCatalog()
        {
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var catalog in this)
            {
                await catalog.Initialize(cancellationToken);
            }

            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }

        public List<ApiDefinition> List()
        {
            var result = new List<ApiDefinition>();

            foreach (var catalog in this)
            {
                var definitionsInCatalog = catalog.List();
                result.AddRange(definitionsInCatalog);
            }

            return result;
        }

        public Api Get(ApiDefinition definition)
        {
            foreach (var catalog in this)
            {
                var api = catalog.Get(definition);

                if (api == null)
                {
                    continue;
                }

                return api;
            }

            return null;
        }
    }
}
