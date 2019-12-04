using System;
using System.Threading.Tasks;
using CodeConfiguration;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public class InitializationTests : ApiFrameworkTestBase
    {
        public InitializationTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }
        
        [Fact]
        public async Task CanInitializeEndpoint()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanInitializeEndpointWithConfiguration()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanInitializeEndpointWithComplexConfiguration()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public async Task CanInitializeEndpointWithRouteParameter()
        {
            throw new NotImplementedException();
        }
    }
}
