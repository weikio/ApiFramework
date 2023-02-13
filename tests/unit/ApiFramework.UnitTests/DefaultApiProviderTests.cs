using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Configuration;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public class DefaultApiProviderTests
    {
        [Fact]
        public async Task CanMatchExact()
        {
            var options = Options.Create(new ApiFrameworkOptions());
            var available = new ApiCatalogForTesting(("A", "1.0.0"), ("B", "2.0.0"));

            var provider = new Weikio.ApiFramework.Core.Apis.DefaultApiProvider(NullLogger<DefaultApiProvider>.Instance, new List<IApiCatalog>() { available },
                options);

            await provider.Initialize(CancellationToken.None);

            var required = new ApiDefinition("A", new Version("1.0.0"));

            var found = provider.Get(required);
        }

        [Fact]
        public async Task ThrowsIfNoMatchFound()
        {
            var options = Options.Create(new ApiFrameworkOptions());

            var available = new ApiCatalogForTesting(("A", "1.0.0"), ("B", "2.0.0"));

            var provider = new Weikio.ApiFramework.Core.Apis.DefaultApiProvider(NullLogger<DefaultApiProvider>.Instance, new List<IApiCatalog>() { available }, options);
            await provider.Initialize(CancellationToken.None);

            var required = new ApiDefinition("C", new Version("1.0.0"));

            Assert.Throws<ApiNotFoundException>(() => provider.Get(required));
        }

        [Fact]
        public async Task CanMatchOnlyExactByDefault()
        {
            var options = Options.Create(new ApiFrameworkOptions());
            var available = new ApiCatalogForTesting(("A", "1.1.0"), ("B", "2.0.0"));

            var provider = new Weikio.ApiFramework.Core.Apis.DefaultApiProvider(NullLogger<DefaultApiProvider>.Instance, new List<IApiCatalog>() { available },
                options);

            await provider.Initialize(CancellationToken.None);

            var required = new ApiDefinition("A", new Version("1.0.0"));

            Assert.Throws<ApiNotFoundException>(() => provider.Get(required));
        }

        [Fact]
        public async Task CanMatchLowest()
        {
            var options = Options.Create(new ApiFrameworkOptions() { ApiVersionMatchingBehaviour = ApiVersionMatchingBehaviour.Lowest });
            var available = new ApiCatalogForTesting(("A", "1.1.0"), ("A", "1.2.0"), ("B", "2.1.0"));

            var provider = new Weikio.ApiFramework.Core.Apis.DefaultApiProvider(NullLogger<DefaultApiProvider>.Instance, new List<IApiCatalog>() { available }, options);
            await provider.Initialize(CancellationToken.None);

            var required = new ApiDefinition("A", new Version("1.0.0"));

            var found = provider.Get(required);
            Assert.Equal(Version.Parse("1.1.0"), found.ApiDefinition.Version);
        }

        [Fact]
        public async Task CanMatchHighest()
        {
            var options = Options.Create(new ApiFrameworkOptions() { ApiVersionMatchingBehaviour = ApiVersionMatchingBehaviour.Highest });
            var available = new ApiCatalogForTesting(("A", "1.1.0"), ("A", "1.2.0"), ("B", "2.1.0"));

            var provider = new Weikio.ApiFramework.Core.Apis.DefaultApiProvider(NullLogger<DefaultApiProvider>.Instance, new List<IApiCatalog>() { available }, options);
            await provider.Initialize(CancellationToken.None);

            var required = new ApiDefinition("A", new Version("1.0.0"));

            var found = provider.Get(required);
            Assert.Equal(Version.Parse("1.2.0"), found.ApiDefinition.Version);
        }
    }
}
