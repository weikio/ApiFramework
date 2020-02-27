using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.ResponceCache;

namespace Weikio.ApiFramework.Samples.NoConfiguration
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            
            services.AddControllers()
                .AddApiFramework(options =>
                {
                    options.AutoResolveApis = true;
                })
                .AddApiFrameworkResponseCache();

            services.AddOpenApiDocument(document => { document.Title = "Api Framework"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseResponseCaching();
            app.UseApiFrameworkResponseCaching();
            
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
