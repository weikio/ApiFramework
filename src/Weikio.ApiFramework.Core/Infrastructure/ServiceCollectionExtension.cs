using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection ConfigureWithDependencies<TOptions, TDependency1>(this IServiceCollection serviceCollection,
            Action<TOptions, TDependency1> configureOptions) where TOptions : class, new()
        {
            serviceCollection.AddSingleton<IConfigureOptions<TOptions>, ConfigureOptionsWithDependencyContainer<TOptions, TDependency1>>();
            serviceCollection.Configure<ConfigureOptionsWithDependency<TOptions, TDependency1>>(options => { options.Action = configureOptions; });

            return serviceCollection;
        }
    }
}
