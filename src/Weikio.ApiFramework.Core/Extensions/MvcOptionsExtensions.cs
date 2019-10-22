using Microsoft.AspNetCore.Mvc;

namespace Weikio.ApiFramework.Core.Extensions
{
    public static class MvcOptionsExtensions
    {
        public static void AddApiFrameworkConventions(this MvcOptions options)
        {
            //services.ConfigureWithDependencies<MvcOptions, SomeConvention>((options, convention) =>
            //{
            //    options.Conventions.Add(convention);
            //});

            //options.Conventions.Add(new FunctionControllerConvention(endpoints));
            //options.Conventions.Add(new FunctionActionConvention(endpoints));
        }
    }
}
