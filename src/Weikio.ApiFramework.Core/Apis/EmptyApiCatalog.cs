using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Apis
{
    public class EmptyApiCatalog : IApiCatalog
    {
        public Task Initialize(CancellationToken cancellationToken)
        {
            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; private set; }

        public List<ApiDefinition> List()
        {
            var result = new List<ApiDefinition>();

            return result;
        }

        public Api Get(ApiDefinition definition)
        {
            return null;
        }
    }
}
