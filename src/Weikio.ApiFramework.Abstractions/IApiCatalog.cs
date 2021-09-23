using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiCatalog
    {
        Task Initialize(CancellationToken cancellationToken);
        bool IsInitialized { get; }

        List<ApiDefinition> List();

        Api Get(ApiDefinition definition);
    }
}