using System.Linq;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core
{
    public class StatusProvider
    {
        private readonly EndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;

        public StatusProvider(EndpointManager endpointManager, IApiProvider apiProvider)
        {
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
        }

        public async Task<Status> Get()
        {
            var result = new Status
            {
                Endpoints = _endpointManager.Endpoints.ToList(),
                EndpointManagerStatusEnum = _endpointManager.Status,
                AvailableApis = await _apiProvider.List()
            };

            return result;
        }
    }
}
