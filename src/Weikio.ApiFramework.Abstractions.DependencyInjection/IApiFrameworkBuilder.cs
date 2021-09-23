using Microsoft.Extensions.DependencyInjection;

namespace Weikio.ApiFramework.Abstractions.DependencyInjection
{
    public interface IApiFrameworkBuilder
    {
        IServiceCollection Services { get; }
    }
}
