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

        List<ApiDefinition> List();

        Api Get(ApiDefinition definition);
        Api Get(string name, Version version);
        Api Get(string name);
    }
}
