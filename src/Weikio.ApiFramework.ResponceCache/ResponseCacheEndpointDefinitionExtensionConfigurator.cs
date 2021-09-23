// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Microsoft.Extensions.Configuration;
// using Weikio.ApiFramework.Abstractions;
//
// namespace Weikio.ApiFramework.Extensions.ResponceCache
// {
//     public class ResponseCacheEndpointDefinitionExtensionConfigurator : IEndpointDefinitionExtensionConfigurator
//     {
//         public void Configure(EndpointDefinition definition, IConfigurationSection configurationSection)
//         {
//             var cacheSection = configurationSection.GetSection("cache");
//
//             if (cacheSection?.GetChildren()?.Any() != true)
//             {
//                 return;
//             }
//
//             var defaultAge = cacheSection.GetValue<TimeSpan>("MaxAge");
//             var vary = cacheSection.GetSection("Vary")?.Get<string[]>() ?? new string[] { };
//             
//             var result = new EndpointResponceCacheOptions();
//
//             var configurations = GetResponseCacheConfigurations(cacheSection);
//
//             foreach (var responseCacheConfiguration in configurations)
//             {
//                 result.AddPathConfiguration(responseCacheConfiguration.Key, responseCacheConfiguration.Value);
//             }
//         }
//         
//         private static Dictionary<string, ResponseCacheConfiguration> GetResponseCacheConfigurations(IConfigurationSection cachingConfigSection)
//         {
//             var cacheConfigs = new Dictionary<string, ResponseCacheConfiguration>(StringComparer.OrdinalIgnoreCase);
//
//             if (cachingConfigSection == null)
//             {
//                 return cacheConfigs;
//             }
//
//             foreach (var configSection in cachingConfigSection.GetChildren())
//             {
//                 var path = configSection.Key;
//                 var maxAge = configSection.GetValue<TimeSpan>("MaxAge");
//                 var vary = configSection.GetSection("Vary")?.Get<string[]>() ?? new string[] { };
//
//                 cacheConfigs.Add(path, new ResponseCacheConfiguration(maxAge, vary));
//             }
//
//             return cacheConfigs;
//         }
//     }
// }


