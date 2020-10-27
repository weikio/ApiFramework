using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using NSwag.Generation.Processors;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.AspNetCore.NSwag;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.OData;
using Weikio.ApiFramework.Plugins.OpenApi;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Samples.CodeConfiguration
{
    public class Startup
    {
        private async Task<string> GetAccessToken()
        {
            var options = new ProcountorOptions();

            var accessParams = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", options.ClientId },
                { "client_secret", options.ClientSecret },
                { "redirect_uri", options.RedirectUri },
                { "api_key", options.ApiKey }
            };

            var accessUrl = $"{options.Url}/oauth/token";

            var accessContent = new FormUrlEncodedContent(accessParams);

            var accessRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(accessUrl, UriKind.Absolute), Method = HttpMethod.Post, Content = accessContent
            };

            var client = GetHttpClient();

            var resAccess = await client.SendAsync(accessRequest);

            var s = await resAccess.Content.ReadAsStringAsync();

            var obj = JObject.Parse(s);

            var accessToken = obj["access_token"].Value<string>();

            return accessToken;
        }

        private static HttpClient GetHttpClient()
        {
            var httpClientHandler = new HttpClientHandler { AllowAutoRedirect = false };
            var result = new HttpClient(httpClientHandler);

            return result;
        }

        public class ProcountorOptions
        {
            public string Company { get; set; } = string.Empty;
            public string ClientId { get; set; } = "adafyClient";
            public string ClientSecret { get; set; } = "secret_LQMVqYV3QMZwHvtouqyJUV8OzfCKWfwcimFdCzHYli6sULs14N";
            public string RedirectUri { get; set; } = "https://proc.adafy.com/integration/auth/redirect";

            public string ApiKey { get; set; } =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIzMzM3OSIsImF1ZCI6ImFkYWZ5Q2xpZW50IiwiaXNzIjoiaHR0cHM6Ly9hcGkucHJvY291bnRvci5jb20iLCJpYXQiOjE2MDI4NDE2NTIsImp0aSI6ImM5ZjBmZWI2LTc3ZTQtNGI4YS04ZGIwLWI1ZTVkYWY3MjRlOCIsImNpZCI6MTQxNDV9.hMcJSiEYVOW9R28PlhgXSNAFIBJy_11b93hz5_VbmQM";

            public string Url { get; set; } = "https://api.procountor.com/latest/api";
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            services.AddRouting();

            var mvcBuilder = services.AddMvc(options => { })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

//            services.AddFunctionFramework(Configuration, mvcBuilder, options =>
//            {
//                options.AutoResolveFunctions = false;
//                options.AutoResolveEndpoints = false;
//            });

            services.AddOData(options =>
                {
                    options.AddModel("odata", GetEdmModel());
                    options.MaxTop = 500;
                }
            );

            IEdmModel GetEdmModel()
            {
                var odataBuilder = new ODataConventionModelBuilder();
                odataBuilder.EntitySet<Customer>("Customer");

                return odataBuilder.GetEdmModel();
            }

            var apiFrameworkBuilder = services.AddApiFramework(options =>
                {
                    options.AutoResolveApis = false;
                    options.AutoResolveEndpoints = false;
                })
                .AddApi(typeof(CustomersApi))
                .AddEndpoint("/Customers", "Weikio.ApiFramework.Samples.CodeConfiguration.CustomersApi")
                // .AddApi(typeof(ApiFactory))
                // .AddEndpoint("/mycustom", "Weikio.ApiFramework.Plugins.OpenApi.ApiFactory",
                //     new ApiOptions()
                //     {
                //         SpecificationUrl = "https://dev.procountor.com/static/swagger.latest.json",
                //         ApiUrl = "https://api.procountor.com/latest/api",
                //         BeforeRequest = async context =>
                //         {
                //             var token = await GetAccessToken();
                //
                //             return token;
                //         },
                //         Mode = ApiMode.Proxy,
                //         ConfigureAdditionalHeaders = (context, state) => new Dictionary<string, string> { { "Authorization", "Bearer " + state } },
                //         IncludeOperation = (operationId, operation, config) =>
                //         {
                //             if (string.Equals(operationId, "get", StringComparison.InvariantCultureIgnoreCase))
                //             {
                //                 return true;
                //             }
                //
                //             return false;
                //         }
                //     })
                .AddOData();

            // }).AddApi(typeof(ApiFactory))
            // .AddEndpoint("/soaptest", "Weikio.ApiFramework.Plugins.Soap.ApiFactory",
            //     configuration: new
            //     {
            //         soapOptions = new SoapOptions()
            //         {
            //             WsdlLocation = "http://localhost:54533/Service1.svc"
            //         }
            //     });

//             services.AddApiFramework(options =>
//                 {
//                     options.AutoResolveEndpoints = false;
//                     options.ApiAddressBase = "/myapi";
//                     options.AutoResolveApis = false;
//
// //
// //                options.Endpoints = new List<(string Route, string FunctionAssemblyName, object Configuration)>()
// //                {
// //                    ("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"}),
// //                    ("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"}),
// //                };
//                 })
//                 
//                 // .AddEndpoint("/withhealth", "Weikio.ApiFramework.Plugins.HealthCheck");
//
// //                .AddEndpoint("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"})
// //                .AddEndpoint("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"});
//                 ;

//             services.AddApiFramework(options =>
//                 {
//                     options.AutoResolveEndpoints = false;
//                     options.ApiAddressBase = "/myapi";
//                     options.AutoResolveApis = false;
//
// //
// //                options.Endpoints = new List<(string Route, string FunctionAssemblyName, object Configuration)>()
// //                {
// //                    ("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"}),
// //                    ("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"}),
// //                };
//                 })
//                 
//                 // .AddEndpoint("/withhealth", "Weikio.ApiFramework.Plugins.HealthCheck");
//
// //                .AddEndpoint("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"})
// //                .AddEndpoint("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"});
//             ;

//            services.AddFunctionFrameworkCore(Configuration, mvcBuilder, options =>
//            {
//                options.AutoResolveEndpoints = false;
//                options.FunctionAddressBase = "/api";
//
//                options.Endpoints = new List<(string Route, string FunctionAssemblyName, object Configuration)>()
//                {
//                    ("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"}),
//                    ("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"}),
//                };
//            }).AddPluginFramework(options =>
//            {
//                options.AutoResolveFunctions = false;
//                options.FunctionAssemblies = new List<string>() {typeof(Weikio.ApiFramework.Plugins.HelloWorld.HelloWorldFunction).Assembly.Location};
//            });

            services.AddOpenApiDocument(document => { document.Title = "Api Framework"; });
            services.AddTransient<IDocumentProcessor, OpenApiExtenderDocumentProcessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            {
                app.UseHsts();
            }

            app.UseRouting();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.Use(next => context =>
            {
                var endpoint = context.GetEndpoint();
                if (endpoint == null)
                {
                    return next(context);
                }

                IEnumerable templates;
                var metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
                if (metadata != null)
                {
                    templates = metadata.Template.GetTemplates();
                }

                return next(context); // put a breaking point here
            });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapHealthChecks("/myhealth",
                        new HealthCheckOptions { Predicate = check => check.Tags.Contains("api_framework_endpoint") });

                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class CustomersApi : ControllerBase
    {
        // [FixedHttpConventions]
        [EnableQuery]
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return new List<Customer>()
            {
                new Customer()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                },
                new Customer()
                {
                    Id = Guid.NewGuid(),
                    Name = "Another",
                },
            };
        }
    }

    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
