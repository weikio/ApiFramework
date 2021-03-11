using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Apis
{
    public class CompositeApiCatalog : List<IApiCatalog>, IApiCatalog
    {
        public CompositeApiCatalog()
        {
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var catalog in this)
            {
                await catalog.Initialize(cancellationToken);
            }

            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }
        public List<ApiDefinition> List()
        {
            var result = new List<ApiDefinition>();

            foreach (var catalog in this)
            {
                var definitionsInCatalog = catalog.List();
                result.AddRange(definitionsInCatalog);
            }

            return result;
        }

        public Api Get(ApiDefinition definition)
        {
            foreach (var catalog in this)
            {
                var api = catalog.Get(definition);

                if (api == null)
                {
                    continue;
                }

                return api;
            }

            return null;
        }
    }

    public class TypeApiCatalog : IApiCatalog
    {
        private readonly Api _api;

        public TypeApiCatalog(Type type)
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

        public List<ApiDefinition> List()
        {
            if (!IsInitialized)
            {
                return new List<ApiDefinition>();
            }
            
            var result = new List<ApiDefinition>(){_api.ApiDefinition};

            return result;
        }

        public Api Get(ApiDefinition definition)
        {
            return _api;
        }
    }
}
