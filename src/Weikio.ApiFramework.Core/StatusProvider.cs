﻿using System.Linq;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core
{
    public class StatusProvider
    {
        private readonly IEndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;

        public StatusProvider(IEndpointManager endpointManager, IApiProvider apiProvider)
        {
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
        }

        public Task<Status> Get()
        {
            var result = new Status
            {
                Endpoints = _endpointManager.Endpoints.ToList(), EndpointManagerStatusEnum = _endpointManager.Status, AvailableApis = _apiProvider.List()
            };

            return Task.FromResult(result);
        }
    }
}
