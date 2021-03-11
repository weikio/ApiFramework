﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiProvider
    {
        Task Initialize(CancellationToken cancellationToken);
        List<ApiDefinition> List();
        Api Get(ApiDefinition definition);
    }
}
