using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class DefaultEndpointHttpVerbResolver : IEndpointHttpVerbResolver
    {
        public string GetHttpVerb(ActionModel action)
        {
            if (action.Attributes.Any(f => f is HttpMethodAttribute))
            {
                var httpMethodAttribute = (HttpMethodAttribute) action.Attributes.First(x => x is HttpMethodAttribute);

                var methods = httpMethodAttribute.HttpMethods?.ToList();

                if (methods != null)
                {
                    if (methods.Contains("POST"))
                    {
                        return "POST";
                    }

                    if (methods.Contains("PUT"))
                    {
                        return "PUT";
                    }

                    if (methods.Contains("DELETE"))
                    {
                        return "DELETE";
                    }
                }
            }

            if (action.ActionName.StartsWith("Create") || action.ActionName.StartsWith("Insert") ||
                action.ActionName.StartsWith("New"))
            {
                return "POST";
            }

            if (action.ActionName.StartsWith("Update") || action.ActionName.StartsWith("Save") ||
                action.ActionName.StartsWith("Replace"))
            {
                return "PUT";
            }

            if (action.ActionName.StartsWith("Remove") || action.ActionName.StartsWith("Delete") ||
                action.ActionName.StartsWith("Del"))
            {
                return "DELETE";
            }

            return "GET";
        }
    }
}
