using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.NuGet;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public static class NugetPackageFactory
    {
        public static NugetPackagePluginCatalog CreatePluginCatalog(string packageName, string version, IServiceProvider provider, string feedUrl = null)
        {
            var options = provider.GetRequiredService<IOptions<PluginFrameworkApiProviderOptions>>().Value;
            var criteria = options.ApiFinderCriteria;

            var catalogOptions = new NugetPluginCatalogOptions()
            {
                TypeFinderOptions = new TypeFinderOptions() { TypeFinderCriterias = criteria },
            };

            NugetPackagePluginCatalog result;

            if (string.IsNullOrWhiteSpace(feedUrl))
            {
                result = new NugetPackagePluginCatalog(packageName, version, true, options: catalogOptions);
            }
            else
            {
                result = new NugetPackagePluginCatalog(packageName, version, true, options: catalogOptions, packageFeed: new NuGetFeed("custom_feed", feedUrl));
            }

            return result;
        }
        
        public static IApiCatalog CreateApiCatalog(string packageName, string version, IServiceProvider provider, string feedUrl = null)
        {
            var pluginCatalog = CreatePluginCatalog(packageName, version, provider, feedUrl);

            var result = new PluginFrameworkApiCatalog(pluginCatalog, provider.GetRequiredService<IApiInitializationWrapper>(),
                provider.GetRequiredService<IApiHealthCheckWrapper>(), provider.GetRequiredService<ILogger<PluginFrameworkApiCatalog>>(),
                provider.GetRequiredService<IOptions<PluginFrameworkApiProviderOptions>>().Value);

            return result;
        }
    }
}
