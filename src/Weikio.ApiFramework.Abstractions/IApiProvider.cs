using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiProvider
    {
        Task Initialize(CancellationToken cancellationToken);
        bool IsInitialized { get; }

        Task<List<ApiDefinition>> List();

        Task<Api> Get(ApiDefinition definition);
        Task<Api> Get(string name, Version version);
        Task<Api> Get(string name);
    }
}
