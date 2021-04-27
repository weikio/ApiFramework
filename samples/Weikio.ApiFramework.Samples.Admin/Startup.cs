using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.ApiFramework.AspNetCore.StarterKit;
using Weikio.ApiFramework.Plugins.SqlServer;

namespace Weikio.ApiFramework.Samples.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddApiFrameworkStarterKit()
                .AddNugetApi("Weikio.ApiFramework.Plugins.OpenApi", "1.0.0-alpha.0.23")
                .AddSqlServer();

            // services.AddApiFramework()
            //     .AddMySql("/data",
            //         new MySqlOptions()
            //         {
            //             ConnectionString =
            //                 "server=192.168.1.11;port=3306;uid=root;pwd=30KWMIl98mAD;database=employees;Convert Zero Datetime=True;Allow Zero Datetime=False",
            //             Tables = new[] { "title*" }
            //         })
            //     .AddSqlServer("/eshop",
            //         new SqlServerOptions()
            //         {
            //             ConnectionString =
            //                 "Server=192.168.1.11;User ID=sa;Password=At6Y1x7AwU7O;Integrated Security=false;Initial Catalog=Microsoft.eShopOnWeb.CatalogDb;",
            //             ExcludedTables = new[] { "__*" }
            //         })
            //     .AddOpenApi("/pets", new ApiOptions()
            //     {
            //         Mode = ApiMode.Proxy,
            //         SpecificationUrl = "https://petstore.swagger.io/v2/swagger.json",
            //         TagTransformMode = TagTransformModeEnum.UseEndpointNameOrRoute
            //     });

            // services.AddResponseCaching();
            // services.AddRouting();
            //
            // var mvcBuilder = services.AddMvc(options =>
            //     {
            //         options.Filters.Add(new ApiConfigurationActionFilter());
            //     })
            //     .SetCompatibilityVersion(CompatibilityVersion.Latest);

            // services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //     .AddCookie();
            //
            // services
            //     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //     .AddJwtBearer();

            // services.AddAuthorization(options =>
            // {
            //     options.AddPolicy("custom", policy =>
            //     {
            //         policy.RequireAuthenticatedUser();
            //
            //         policy.RequireAssertion(context =>
            //         {
            //             var res = false;
            //
            //             return res;
            //         });
            //     });
            // });

            // services.AddApiFramework(options =>
            //     {
            //         options.AutoResolveEndpoints = false;
            //         options.AutoResolveApis = false;
            //     })
            //     .AddApi(typeof(ApiFactory))
            //     .AddApi(typeof(HelloWorldApi))
            //     .AddApi(typeof(NewJsonApi))
            //     .AddApi(typeof(OldJsonApi))
            //     .AddApi(typeof(Weikio.ApiFramework.Plugins.DynamicHelloWorld.ApiFactory))
            //     .AddApi(typeof(SometimesWorkingApi))
            //     .AddEndpoint("/sometimesworks", "HealthCheck")
            //     .AddEndpoint("/notworking", "Broken", healthCheck: new CustomHealthCheck())
            //     .AddEndpoint("/working", "HelloWorld", new { HelloString = "Fast Hellou!!!" })
            //     .AddEndpoint("/working2", "HelloWorld", new { HelloString = "Another configuration Hellou!!!" })
            //     .AddAdmin(options =>
            //     {
            //         options.EndpointApiPolicy = "custom";
            //         options.EndpointAdminRouteRoot = "myadmin/here/itis";
            //     });

            // services.AddOpenApiDocument(document =>
            // {
            //     document.Title = "Api Framework";
            //     document.DocumentName = "api";
            //     // document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor("api_framework_endpoint"));
            // });
            //
            // services.AddOpenApiDocument(document =>
            // {
            //     document.Title = "Api Framework";
            //     document.DocumentName = "external";
            //     // document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor("external"));
            // });
            //
            // services.AddOpenApiDocument(document =>
            // {
            //     document.Title = "Api Framework";
            //     document.ApiGroupNames = new[] { "api_framework_admin" };
            //     document.DocumentName = "api_admin";
            // });
            //


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
