using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiFramework.IntegrationTests.Infrastructure;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework;
using Weikio.ApiFramework.Core.Apis;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class ApiCatalogTests : ApiFrameworkTestBase
    {
        public ApiCatalogTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanStartWithoutCatalogs()
        {
            var server = Init(builder =>
            {
            });

            var apiProvider = Provider.GetRequiredService<IApiProvider>();
            var catalogs = apiProvider.ListCatalogs();
            var apis = catalogs.SelectMany(x => x.List());

            Assert.Empty(apis);
        }

        [Fact]
        public void CanAddCatalogUsingBuilder()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
            });

            var apiProvider = Provider.GetRequiredService<IApiProvider>();
            var catalogs = apiProvider.ListCatalogs();

            Assert.NotEmpty(catalogs.SelectMany(x => x.List()));
        }

        [Fact]
        public void CanAddCatalogUsingOptions()
        {
            var server = Init(builder =>
                {
                },
                options =>
                {
                    options.ApiCatalogs.Add(new TypeApiCatalog(typeof(HelloWorldApi)));
                });

            var apiProvider = Provider.GetRequiredService<IApiProvider>();
            var catalogs = apiProvider.ListCatalogs();

            Assert.NotEmpty(catalogs.SelectMany(x => x.List()));
        }

        [Fact]
        public async Task CanAddCatalogRuntime()
        {
            var server = Init(builder =>
            {
            });

            var apiProvider = Provider.GetRequiredService<IApiProvider>();
            var catalogs = apiProvider.ListCatalogs();
            
            Assert.Empty(catalogs.SelectMany(x => x.List()));

            var typeCatalog = new TypeApiCatalog(typeof(HelloWorldApi));
            await typeCatalog.Initialize(new CancellationToken());
            
            apiProvider.Add(typeCatalog);
            
            Assert.NotEmpty(catalogs.SelectMany(x => x.List()));
        }
        
        [Fact]
        public void ThrowsIfAddedCatalogIsNotInitialized()
        {
            Init(builder =>
            {
            });

            var apiProvider = Provider.GetRequiredService<IApiProvider>();
            var catalogs = apiProvider.ListCatalogs();
            
            Assert.Empty(catalogs.SelectMany(x => x.List()));

            var typeCatalog = new TypeApiCatalog(typeof(HelloWorldApi));

            Assert.Throws<ApiCatalogNotInitializedException>(() => apiProvider.Add(typeCatalog));
        }
        
        [Fact]
        public async Task CanRemoveCatalogRuntime()
        {
            var server = Init(builder =>
            {
            });

            var apiProvider = Provider.GetRequiredService<IApiProvider>();
            var catalogs = apiProvider.ListCatalogs();
            
            var typeCatalog = new TypeApiCatalog(typeof(HelloWorldApi));
            await typeCatalog.Initialize(new CancellationToken());
            
            apiProvider.Add(typeCatalog);
            Assert.NotEmpty(catalogs.SelectMany(x => x.List()));
            
            apiProvider.Remove(typeCatalog);
            Assert.Empty(catalogs.SelectMany(x => x.List()));
        }
    }
}
