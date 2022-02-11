using System;
using System.Threading;
using System.Threading.Tasks;
using HelloWorld;
using Microsoft.Extensions.Logging.Abstractions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.TypeFinding;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    [Collection(nameof(NotThreadSafeResourceCollection))]
    public class TypeApiTests : IDisposable
    {
        public TypeApiTests()
        {
            var opt = new PluginFrameworkApiProviderOptions();
            TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();

            foreach (var apiFinderCriterion in opt.ApiFinderCriteria)
            {
                TypeFinderOptions.Defaults.TypeFinderCriterias.Add(apiFinderCriterion);
            }
        }

        [Fact]
        public async Task CanInitializeTypeBasedApi()
        {
            var provider = new PluginFrameworkApiCatalog(new TypePluginCatalog(typeof(HelloWorldApi)),
                new ApiInitializationWrapperForUnitTests(),
                new ApiHealthCheckWrapperForUnitTests(), new NullLogger<PluginFrameworkApiCatalog>(), new PluginFrameworkApiProviderOptions());

            await provider.Initialize(new CancellationToken());

            var all = provider.List();

            // Assert
            Assert.NotEmpty(all);
        }

        [Fact]
        public async Task CanGetTypeBasedApi()
        {
            var provider = new PluginFrameworkApiCatalog(new TypePluginCatalog(typeof(HelloWorld.HelloWorldApi)),
                new ApiInitializationWrapperForUnitTests(),
                new ApiHealthCheckWrapperForUnitTests(), new NullLogger<PluginFrameworkApiCatalog>(), new PluginFrameworkApiProviderOptions());

            await provider.Initialize(new CancellationToken());

            // Assert doesn't throw
            provider.Get((typeof(HelloWorld.HelloWorldApi).FullName));
        }

        [Fact]
        public async Task TypeBasedApiShouldHaveOnlyOneApi()
        {
            var provider = new PluginFrameworkApiCatalog(new TypePluginCatalog(typeof(HelloWorldApi)),
                new ApiInitializationWrapperForUnitTests(),
                new ApiHealthCheckWrapperForUnitTests(), new NullLogger<PluginFrameworkApiCatalog>(), new PluginFrameworkApiProviderOptions());

            await provider.Initialize(new CancellationToken());

            var api = provider.Get((typeof(HelloWorld.HelloWorldApi).FullName));

            // Assert
            Assert.Single(api.ApiTypes);
        }

        public void Dispose()
        {
            TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
        }
    }
}
