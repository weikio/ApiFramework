using System.Text;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Weikio.ApiFramework.Admin.Areas.Admin.Controllers;

namespace Weikio.ApiFramework.Admin
{
    public class ApiFrameworkAdminActionConvention : IActionModelConvention
    {
        private readonly string _policyName;

        public ApiFrameworkAdminActionConvention(string policyName)
        {
            _policyName = policyName;
        }

        public void Apply(ActionModel action)
        {
            if (string.IsNullOrWhiteSpace(_policyName))
            {
                return;
            }

            if (action.Controller.ControllerType != typeof(EndpointsController))
            {
                return;
            }

            action.Filters.Add(new AuthorizeFilter(_policyName));
        }
    }
}
