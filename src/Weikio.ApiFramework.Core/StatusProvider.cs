using System.Linq;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core
{
    public class StatusProvider
    {
        private readonly EndpointManager _endpointManager;
        private readonly IFunctionProvider _functionProvider;

        public StatusProvider(EndpointManager endpointManager, IFunctionProvider functionProvider)
        {
            _endpointManager = endpointManager;
            _functionProvider = functionProvider;
        }

        public async Task<Status> Get()
        {
            var result = new Status
            {
                Endpoints = _endpointManager.Endpoints.ToList(),
                EndpointManagerStatusEnum = _endpointManager.Status,
                AvailableFunctions = await _functionProvider.List()
            };

            return result;
        }
    }
}
