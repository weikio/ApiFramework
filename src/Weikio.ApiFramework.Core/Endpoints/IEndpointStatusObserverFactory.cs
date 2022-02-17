using System;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public interface IEndpointStatusObserverFactory
    {
        IStatusObserver<EndpointStatusEnum> Create(IServiceProvider serviceProvider, Endpoint endpoint);
    }
}
