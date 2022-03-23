using System;
using Weikio.ApiFramework.Abstractions;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public class EndpointDefinitionTests
    {
        [Fact]
        public void CannotDefineNullRoute()
        {
            Assert.Throws<ArgumentNullException>("route", () =>
            {
                new EndpointDefinition(route: null, api: null);
            });
        }

        [Fact]
        public void CannotDefineEmptyRoute()
        {
            Assert.Throws<ArgumentException>("route", () =>
            {
                new EndpointDefinition(route: "", api: null);
            });
        }

        [Fact]
        public void CannotDefineWhitespaceRoute()
        {
            Assert.Throws<ArgumentException>("route", () =>
            {
                new EndpointDefinition(route: " ", api: null);
            });
        }

        [Fact]
        public void CannotDefineNullApi()
        {
            Assert.Throws<ArgumentNullException>("api", () =>
            {
                new EndpointDefinition(route: "/test", api: null);
            });
        }
    }
}
