using Microsoft.Extensions.DependencyInjection;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiFrameworkBuilder
    {
        IServiceCollection Services { get; }
    }
}
