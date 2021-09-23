using System.Threading;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiChangeToken
    {
        public void Initialize()
        {
            TokenSource = new CancellationTokenSource();
        }

        public CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();
    }
}
