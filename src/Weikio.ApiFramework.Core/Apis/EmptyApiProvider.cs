using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Apis
{
    public class EmptyApiProvider : IApiProvider
    {
        public Task Initialize(CancellationToken cancellationToken)
        {
            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; private set; }
        public List<ApiDefinition> List()
        {
            var result = new List<ApiDefinition>();

            return result;
        }

        public Api Get(ApiDefinition definition)
        {
            throw new ApiNotFoundException(definition.Name, definition.Version);
        }

        public Api Get(string name, Version version)
        {
            return Get(new ApiDefinition(name, version));
        }

        public Api Get(string name)
        {
            return Get(name, new Version(1, 0, 0, 0));
        }
    }
}
