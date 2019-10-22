using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.AspNetCore.StartupTasks
{
    public class DelayStartupTask : IStartupTask
    {
        public async Task Execute(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }
    }
}
