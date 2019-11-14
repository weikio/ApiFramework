using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class DefaultEndpointHttpVerbResolver : IEndpointHttpVerbResolver
    {
        public string GetHttpVerb(ActionModel action)
        {
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
