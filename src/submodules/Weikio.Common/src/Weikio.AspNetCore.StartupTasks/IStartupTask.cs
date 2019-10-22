using System.Threading;
using System.Threading.Tasks;

namespace Weikio.AspNetCore.StartupTasks
{
    public interface IStartupTask
    {
        Task Execute(CancellationToken cancellationToken);
    }
}
