// using System;
// using System.Linq;
// using NSwag.Generation.AspNetCore;
// using NSwag.Generation.Processors;
// using NSwag.Generation.Processors.Contexts;
// using Weikio.ApiFramework.Abstractions;
//
// namespace Weikio.ApiFramework.Samples.Admin
// {
//     public class ApiFrameworkTagOperationProcessor : IOperationProcessor
//     {
//         private readonly string[] _groups;
//
//         public ApiFrameworkTagOperationProcessor(string group) : this(new[] { group })
//         {
//         }
//
//         public ApiFrameworkTagOperationProcessor(string[] groups)
//         {
//             _groups = groups;
//         }
//
//         public bool Process(OperationProcessorContext context)
//         {
//             if (!(context is AspNetCoreOperationProcessorContext aspnetContext))
//             {
//                 return false;
//             }
//
//             var tags = context.OperationDescription.Operation.Tags;
//
//             var apiDescription =
//                 (Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription) typeof(AspNetCoreOperationProcessorContext).GetProperty("ApiDescription")
//                     .GetValue(aspnetContext);
//             var endPoint = apiDescription.ActionDescriptor.EndpointMetadata?.OfType<Endpoint>().FirstOrDefault();
//
//             if (endPoint == null)
//             {
//                 return false;
//             }
//
//             var found = false;
//
//             if (endPoint.GroupName?.Any() == true)
//             {
//                 foreach (var groupName in endPoint.GroupName)
//                 {
//                     if (_groups.Contains(groupName, StringComparer.InvariantCultureIgnoreCase))
//                     {
//                         found = true;
//
//                         break;
//                     }
//                 }
//             }
//
//             if (!found)
//             {
//                 return false;
//             }
//
//             tags.Clear();
//
//             var route = endPoint.Route.Trim('/').Trim();
//             var routeInUpper = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(route);
//
//             tags.Add(routeInUpper);
//
//             // else if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
//             // {
//             //     if (!controllerActionDescriptor.ControllerTypeInfo.Assembly.FullName.Contains("Weikio.ApiFramework.Admin"))
//             //     {
//             //         return true;
//             //     }
//             //
//             //     tags.Clear();
//             //     tags.Add("Admin");
//             // }
//
//             return true;
//         }
//     }
// }
