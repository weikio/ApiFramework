namespace Weikio.ApiFramework.SDK
{
    public static class ApiFrameworkPluginExtensions
    {
        // public static IApiFrameworkBuilder RegisterPlugin<TPluginType>(this IApiFrameworkBuilder builder, Action<EndpointDefinition> configureEndpoint = null)
        // {
        //     builder.Services.RegisterPlugin<TPluginType>(configureEndpoint);
        //
        //     return builder;
        // }
        //
        // public static IServiceCollection RegisterPlugin<TPluginType>(this IServiceCollection services, Action<EndpointDefinition> configureEndpoint = null)
        // {
        //     var assembly = typeof(TPluginType).Assembly;
        //     
        //     var apiPlugin = new ApiPlugin { Assembly = assembly };
        //
        //     services.AddSingleton(typeof(ApiPlugin), apiPlugin);
        //
        //     services.Configure<ApiPluginOptions>(options =>
        //     {
        //         if (options.ApiPluginAssemblies.Contains(assembly))
        //         {
        //             return;
        //         }
        //
        //         options.ApiPluginAssemblies.Add(assembly);
        //     });
        //
        //     if (configureEndpoint != null)
        //     {
        //         var endpointDefinition = new EndpointDefinition();
        //         configureEndpoint(endpointDefinition);
        //
        //         services.AddSingleton(endpointDefinition);
        //     }
        //     
        //     return services;
        // }
    }
}