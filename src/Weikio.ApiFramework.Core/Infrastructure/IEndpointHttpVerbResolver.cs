using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public interface IEndpointHttpVerbResolver
    {
        string GetHttpVerb(ActionModel action);
    }
}
