using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Apis
{
    public class TypeApiProvider : IApiProvider
    {
        private readonly Api _api;

        public TypeApiProvider(Type type)
        {
            var apiDefinition = new ApiDefinition(type.FullName, Version.Parse("1.0.0.0"));
            _api = new Api(apiDefinition, new List<Type>() { type });
        }

        public Task Initialize(CancellationToken cancellationToken)
        {
            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; private set; }

        public Task<List<ApiDefinition>> List()
        {
            if (!IsInitialized)
            {
                return Task.FromResult(new List<ApiDefinition>());
            }
            
            var result = new List<ApiDefinition>(){_api.ApiDefinition};

            return Task.FromResult(result);
        }

        public Task<Api> Get(ApiDefinition definition)
        {
            return Task.FromResult(_api);
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
