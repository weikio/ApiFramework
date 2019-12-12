using System.Threading;
using System.Threading.Tasks;
using HelloWorld;
using Microsoft.Extensions.Logging.Abstractions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public class TypeApiTests
    {
        [Fact]
        public async Task CanInitializeTypeBasedApi()
        {
            var provider = new PluginFrameworkApiProvider(new TypePluginCatalog(typeof(HelloWorldApi)), new PluginExporter(),
                new ApiInitializationWrapperForUnitTests(),
                new ApiHealthCheckWrapperForUnitTests(), new NullLogger<PluginFrameworkApiProvider>());

            await provider.Initialize(new CancellationToken());

            var all = await provider.List();
            
            // Assert
            Assert.NotEmpty(all);
        }
        
        [Fact]
        public async Task CanGetTypeBasedApi()
        {
            var provider = new PluginFrameworkApiProvider(new TypePluginCatalog(typeof(HelloWorldApi)), new PluginExporter(),
                new ApiInitializationWrapperForUnitTests(),
                new ApiHealthCheckWrapperForUnitTests(), new NullLogger<PluginFrameworkApiProvider>());

            await provider.Initialize(new CancellationToken());

            // Assert doesn't throw
            await provider.Get("HelloWorld");
        }
        
        [Fact]
        public async Task TypeBasedApiShouldHaveOnlyOneApi()
        {
            var provider = new PluginFrameworkApiProvider(new TypePluginCatalog(typeof(HelloWorldApi)), new PluginExporter(),
                new ApiInitializationWrapperForUnitTests(),
                new ApiHealthCheckWrapperForUnitTests(), new NullLogger<PluginFrameworkApiProvider>());

            await provider.Initialize(new CancellationToken());

            var api = await provider.Get("HelloWorld");

            // Assert
            Assert.Single(api.ApiTypes);
        }
    }
}
