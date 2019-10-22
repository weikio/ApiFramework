using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core.Extensions
{
    public static class ApplicationPartManagerExtensions
    {
        public static void AddApiFrameworkFeatures(this ApplicationPartManager manager, System.Func<EndpointManager> endpoints)
        {
            //manager.FeatureProviders.Add(new FunctionFeatureProvider(endpoints));
        }
    }
}
