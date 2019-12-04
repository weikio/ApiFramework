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
    public class AssemblyApiTests
    {
        [Fact]
        public async Task CanInitializeAssemblyBasedApi()
        {
            var provider = new PluginFrameworkApiProvider(new AssemblyPluginCatalog(typeof(HelloWorldApi).Assembly), new PluginExporter(),
                new ApiInitializationWrapperForUnitTests(),
                new ApiHealthCheckWrapperForUnitTests(), new NullLogger<PluginFrameworkApiProvider>());

            await provider.Initialize(new CancellationToken());

            var all = await provider.List();
            
            Assert.NotEmpty(all);
        }
    }
}
