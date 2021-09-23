using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Apis
{
    public class TypeApiCatalog : IApiCatalog
    {
        private readonly Api _api;

        public TypeApiCatalog(Type type)
        {
            var apiDefinition = new ApiDefinition(type.FullName, Version.Parse("1.0.0.0"));
            _api = new Api(apiDefinition, new List<Type>() { type }, type.Assembly);
        }

        public Task Initialize(CancellationToken cancellationToken)
        {
            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; private set; }

        public List<ApiDefinition> List()
        {
            if (!IsInitialized)
            {
                return new List<ApiDefinition>();
            }

            var result = new List<ApiDefinition>() { _api.ApiDefinition };

            return result;
        }

        public Api Get(ApiDefinition definition)
        {
            return _api;
        }
    }
}
