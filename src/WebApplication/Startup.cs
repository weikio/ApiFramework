using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.AspNetCore.StarterKit;
using Weikio.ApiFramework.AspNetCore.StarterKit.Middlewares;
using Weikio.ApiFramework.Core.Extensions;

namespace WebApplication
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
            services.AddLogging();

            services.AddSingleton<ILogger>(provider =>
            {
                var fact = provider.GetRequiredService<ILoggerFactory>();

                return fact.CreateLogger(typeof(Startup));
            });

            services.AddApiFrameworkWithAdmin()
                .AddApi(typeof(ApiFactory))
                .AddEndpoint("/test", "WebApplication.ApiFactory", new CalcConfiguration()
                {
                    Y = 15
                })
                .AddEndpoint("/test2", "WebApplication.ApiFactory", new CalcConfiguration()
                {
                    Y = 20
                });

            services.Configure<ConditionalMiddlewareOptions>(opt =>
            {
                opt.Configure = appBuilder =>
                {
                    appBuilder.Use(async (context, next) =>
                    {
                        // Do work that doesn't write to the Response.
                        await next.Invoke();
                        var logger = context.RequestServices.GetRequiredService<ILogger>();
                        logger.LogInformation("Run after middleware");
                    });
                };
            });
            
            // services.AddConditionalMiddlewareBeforeEndpoints();

            // .AddMySql("/data",
            //     new MySqlOptions()
            //     {
            //         ConnectionString =
            //             "server=192.168.1.11;port=3306;uid=root;pwd=30KWMIl98mAD;database=employees;Convert Zero Datetime=True;Allow Zero Datetime=False",
            //         Tables = new[] { "title*" }
            //     })
            // .AddSqlServer("/eshop",
            //     new SqlServerOptions()
            //     {
            //         ConnectionString =
            //             "Server=192.168.1.11;User ID=sa;Password=At6Y1x7AwU7O;Integrated Security=false;Initial Catalog=Microsoft.eShopOnWeb.CatalogDb;",
            //         ExcludedTables = new[] { "__*" }
            //     })
            // .AddOpenApi("/pets", new ApiOptions() { SpecificationUrl = "https://petstore.swagger.io/v2/swagger.json", });

            // .AddApi(new Func<string, string>(name => $"Hello there {name}"), "MyApi");
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class CalcConfiguration
    {
        public int Y { get; set; }
    }

    public class Calc
    {
        public CalcConfiguration Configuration { get; set; }
        
        public int Calculate(int i, int x)
        {
            return i + x + Configuration.Y;
        }
    }

    public static class ApiFactory
    {
        public static Task<IEnumerable<Type>> Create()
        {
            return Task.FromResult<IEnumerable<Type>>(new List<Type>() { typeof(Calc) });
        }
    }
}
