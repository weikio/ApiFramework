using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Apis
{
    public class EmptyApiProvider : IApiProvider
    {
        public Task Initialize()
        {
            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; private set; }
        public Task<List<ApiDefinition>> List()
        {
            var result = new List<ApiDefinition>();

            return Task.FromResult(result);
        }

        public Task<Api> Get(ApiDefinition definition)
        {
            throw new ApiNotFoundException(definition.Name, definition.Version);
        }

        public Task<Api> Get(string name, Version version)
        {
            return Get(new ApiDefinition(name, version));
        }

        public Task<Api> Get(string name)
        {
            return Get(name, new Version(1, 0, 0, 0));
        }
    }
}
