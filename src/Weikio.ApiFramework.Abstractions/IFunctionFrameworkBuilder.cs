using Microsoft.Extensions.DependencyInjection;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IFunctionFrameworkBuilder
    {
        IServiceCollection Services { get; }
    }
}
