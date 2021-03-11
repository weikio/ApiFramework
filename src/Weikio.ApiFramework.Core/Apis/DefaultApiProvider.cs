using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Apis
{
    public class DefaultApiProvider : List<IApiCatalog>, IApiProvider
    {
        private readonly ILogger<DefaultApiProvider> _logger;

        public DefaultApiProvider(ILogger<DefaultApiProvider> logger, IEnumerable<IApiCatalog> apiProviders)
        {
            _logger = logger;
            AddRange(apiProviders);
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing Api Providers");

            foreach (var provider in this)
            {
                await provider.Initialize(cancellationToken);
            }
        }

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

            var allDefinitions = List();

            throw new ApiNotFoundException(definition.Name, definition.Version,
                $"No API found with definition {definition}. Available APIs:{Environment.NewLine}{string.Join(Environment.NewLine, allDefinitions)}");
        }
    }
}
