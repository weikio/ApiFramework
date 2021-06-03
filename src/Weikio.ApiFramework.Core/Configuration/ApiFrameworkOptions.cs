using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Infrastructure;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class ApiFrameworkOptions
    {
        /// <summary>
        /// Gets or sets the base path for API Framework endpoints
        /// </summary>
        public string ApiAddressBase { get; set; } = "/api";

        public IEndpointHttpVerbResolver EndpointHttpVerbResolver { get; set; }

        public bool AutoResolveEndpoints { get; set; } = false;

        public List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck HealthCheck, string GroupName)> Endpoints { get; set; } =
            new List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck HealthCheck, string groupName)>();

        public List<IApiCatalog> ApiCatalogs { get; set; } = new List<IApiCatalog>();

        /// <summary>
        /// Gets or sets how endpoint changes are notified to the system. 
        /// </summary>
        public ChangeNotificationTypeEnum ChangeNotificationType { get; set; } = ChangeNotificationTypeEnum.Single;

        public bool AutoInitializeApiProvider { get; set; } = true;
        public bool AutoInitializeConfiguredEndpoints { get; set; } = true;
        public bool AutoConvertFileToStream { get; set; } = true;

        /// <summary>
        /// Gets or sets if urls should be automatically tidied. Default = Automatic
        /// </summary>
        public AutoTidyUrlModeEnum AutoTidyUrls { get; set; } = AutoTidyUrlModeEnum.Automatic;
    }
}
