using System.Collections;
using System.Collections.Generic;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public class EndpointCollection : IEnumerable<Endpoint>
    {
        private readonly IEnumerable<Endpoint> _endpoints;

        public EndpointCollection(IEnumerable<Endpoint> endpoints)
        {
            _endpoints = endpoints;
        }

        public IEnumerator<Endpoint> GetEnumerator()
        {
            return _endpoints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _endpoints.GetEnumerator();
        }
    }
}
