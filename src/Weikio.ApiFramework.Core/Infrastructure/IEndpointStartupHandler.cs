using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public interface IEndpointStartupHandler
    {
        void Start(CancellationToken cancellationToken);
    }
}
