using System.Threading.Tasks;

namespace Weikio.ApiFramework.Core.StartupTasks
{
    public interface IApiProviderInitializer
    {
        Task Initialize();
    }
}
