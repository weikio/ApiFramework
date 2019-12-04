using System;
using System.Threading.Tasks;
using CodeConfiguration;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public class EndpointConfigurationTests : ApiFrameworkTestBase
    {
        public EndpointConfigurationTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }
        
        [Fact]
        public async Task CanSetConfiguration()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanSetComplexConfiguration()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public async Task CanSetAnonymousConfiguration()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public async Task CanSetRouteToConfiguration()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanSetDifferentConfigurationToDifferentEndpoints()
        {
            throw new NotImplementedException();
        }
    }
}
