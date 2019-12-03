using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Core.StartupTasks
{
    public interface IApiProviderInitializer
    {
        void Initialize();
    }
}
