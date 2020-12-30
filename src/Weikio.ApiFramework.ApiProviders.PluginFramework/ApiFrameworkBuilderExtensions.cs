using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.Delegates;
using Weikio.PluginFramework.Catalogs.Roslyn;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public static class ApiFrameworkBuilderExtensions
    {
        public static IApiFrameworkBuilder AddApiCatalog(this IApiFrameworkBuilder builder, IPluginCatalog catalog)
        {
            builder.Services.AddSingleton<IPluginCatalog>(catalog);

            return builder;
        }
        
        public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, string roslynCode, string apiName,
            Version version = null, RoslynPluginCatalogOptions catalogOptions = null)
        {
            if (string.IsNullOrWhiteSpace(apiName))
            {
                throw new ArgumentNullException(apiName);
            }
            
            if (string.IsNullOrWhiteSpace(roslynCode))
            {
                throw new ArgumentNullException(roslynCode);
            }

            if (catalogOptions == null)
            {
                catalogOptions = new RoslynPluginCatalogOptions();
            }
            
            if (catalogOptions.Tags?.Any() != true)
            {
                catalogOptions.Tags = new List<string>() { "Api" };
            }

            catalogOptions.PluginName = apiName;
            catalogOptions.PluginVersion = version == null ? new Version(1, 0, 0, 0) : version;
            //
            // if (catalogOptions.ConversionRules?.Any() != true)
            // {
            //     catalogOptions.ConversionRules = new List<DelegateConversionRule>();
            // }
            //
            // catalogOptions.ConversionRules.Add(new DelegateConversionRule(info => info.Name == "configuration",
            //     nfo => new ParameterConversion() { ToPublicProperty = true }));
            //
            // catalogOptions.ConversionRules.Add(new DelegateConversionRule(info => info.Name == "config",
            //     nfo => new ParameterConversion() { ToPublicProperty = true }));
            //
            // var catalog = new DelegatePluginCatalog(multicastDelegate, catalogOptions);

            var catalog = new RoslynPluginCatalog(roslynCode, catalogOptions);
            builder.Services.AddSingleton<IPluginCatalog>(catalog);

            return builder;
        }

        public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, MulticastDelegate multicastDelegate, string apiName,
            Version version = null, DelegatePluginCatalogOptions catalogOptions = null)
        {
            if (string.IsNullOrWhiteSpace(apiName))
            {
                throw new ArgumentNullException(apiName);
            }

            if (multicastDelegate == null)
            {
                throw new ArgumentNullException(nameof(multicastDelegate));
            }

            if (catalogOptions == null)
            {
                catalogOptions = new DelegatePluginCatalogOptions();
            }

            if (catalogOptions.Tags?.Any() != true)
            {
                catalogOptions.Tags = new List<string>() { "Api" };
            }

            catalogOptions.NameOptions = new PluginNameOptions()
            {
                PluginNameGenerator = (options, type) => apiName,
                PluginVersionGenerator = (options, type) => version == null ? new Version(1, 0, 0, 0) : version
            };

            if (catalogOptions.ConversionRules?.Any() != true)
            {
                catalogOptions.ConversionRules = new List<DelegateConversionRule>();
            }

            catalogOptions.ConversionRules.Add(new DelegateConversionRule(info => info.Name == "configuration",
                nfo => new ParameterConversion() { ToPublicProperty = true }));

            catalogOptions.ConversionRules.Add(new DelegateConversionRule(info => info.Name == "config",
                nfo => new ParameterConversion() { ToPublicProperty = true }));

            var catalog = new DelegatePluginCatalog(multicastDelegate, catalogOptions);

            builder.Services.AddSingleton<IPluginCatalog>(catalog);

            return builder;
        }
    }
}
