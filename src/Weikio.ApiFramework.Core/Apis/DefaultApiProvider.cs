using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Resolver;
using NuGet.Versioning;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;

[assembly:InternalsVisibleTo("ApiFramework.UnitTests")]

namespace Weikio.ApiFramework.Core.Apis
{
    internal class DefaultApiProvider : List<IApiCatalog>, IApiProvider
    {
        private readonly ILogger<DefaultApiProvider> _logger;
        private readonly IOptions<ApiFrameworkOptions> _options;

        public DefaultApiProvider(ILogger<DefaultApiProvider> logger, IEnumerable<IApiCatalog> apiCatalogs, IOptions<ApiFrameworkOptions> options)
        {
            _logger = logger;
            _options = options;
            AddRange(apiCatalogs);
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing Api Providers");

            foreach (var provider in this.Where(x => x.IsInitialized == false))
            {
                await provider.Initialize(cancellationToken);
            }
        }

        public List<ApiDefinition> List()
        {
            var result = new List<ApiDefinition>();

            foreach (var catalog in this.Where(x => x.IsInitialized))
            {
                var definitionsInCatalog = catalog.List();
                result.AddRange(definitionsInCatalog);
            }

            return result;
        }

        public List<IApiCatalog> ListCatalogs()
        {
            return this;
        }

        public Api Get(ApiDefinition definition)
        {
            var allDefinitions = List();

            var whatIsNeeded = CreatePackage("fixed_not_existing", "1.0", new Dictionary<string, string>()
            {
                { definition.Name, definition.Version.ToString() }
            });
            
            var whatIsAvailable = new List<ResolverPackage>()
            {
                whatIsNeeded
            };

            foreach (var def in allDefinitions)
            {
                whatIsAvailable.Add(new ResolverPackage(def.Name, NuGetVersion.Parse(def.Version.ToString())));
            }
           
            var resolver = new PackageResolver();

            try
            {
                var matchingBehavior = _options.Value.ApiVersionMatchingBehaviour;
                var tryingToFind = definition;
                
                if (matchingBehavior != ApiVersionMatchingBehaviour.OnlyExact)
                {
                    var nugetDepBehavior = DependencyBehavior.Lowest;

                    if (matchingBehavior == ApiVersionMatchingBehaviour.Lowest)
                    {
                        nugetDepBehavior = DependencyBehavior.Lowest;
                    }
                    else if (matchingBehavior == ApiVersionMatchingBehaviour.HighestPatch)
                    {
                        nugetDepBehavior = DependencyBehavior.HighestPatch;
                    }
                    else if (matchingBehavior == ApiVersionMatchingBehaviour.HighestMinor)
                    {
                        nugetDepBehavior = DependencyBehavior.HighestMinor;
                    }
                    else if (matchingBehavior == ApiVersionMatchingBehaviour.Highest)
                    {
                        nugetDepBehavior = DependencyBehavior.Highest;
                    }
                    
                    var resolverContext =
                        CreatePackageResolverContext(nugetDepBehavior, whatIsNeeded, whatIsAvailable);
                    
                    var packages = resolver.Resolve(resolverContext, CancellationToken.None).ToDictionary(p => p.Id);
                    var found = packages.First();
                    
                    tryingToFind = new ApiDefinition(found.Value.Id, Version.Parse(found.Value.Version.OriginalVersion));
                }
                
                foreach (var catalog in this.Where(x => x.IsInitialized))
                {
                    var api = catalog.Get(tryingToFind);

                    if (api == null)
                    {
                        continue;
                    }

                    return api;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Couldn't locate API Definition {ApiDefinition}", definition);
            }
            
            throw new ApiNotFoundException(definition.Name, definition.Version,
                $"No API found with definition {definition}. Available APIs:{Environment.NewLine}{string.Join(Environment.NewLine, allDefinitions)}");
        }

        public new void Add(IApiCatalog catalog)
        {
            base.Add(catalog);
        }

        public new void Remove(IApiCatalog catalog)
        {
            base.Remove(catalog);
        }
        
        // Version matching based on the Nuget unit tests
        private static ResolverPackage CreatePackage(string id, string version, IDictionary<string, string> dependencies = null)
        {
            var deps = new List<NuGet.Packaging.Core.PackageDependency>();

            if (dependencies != null)
            {
                foreach (var dep in dependencies)
                {
                    VersionRange range = null;

                    if (dep.Value != null)
                    {
                        range = VersionRange.Parse(dep.Value);
                    }

                    deps.Add(new NuGet.Packaging.Core.PackageDependency(dep.Key, range));
                }
            }

            return new ResolverPackage(id, NuGetVersion.Parse(version), deps, true, false);
        }
        
        private static PackageResolverContext CreatePackageResolverContext(DependencyBehavior behavior,
            PackageIdentity target,
            IEnumerable<ResolverPackage> availablePackages)
        {
            var targets = new PackageIdentity[] { target };

            return CreatePackageResolverContext(behavior, targets, availablePackages);
        }
        
        private static PackageResolverContext CreatePackageResolverContext(DependencyBehavior behavior,
            IEnumerable<PackageIdentity> targets,
            IEnumerable<ResolverPackage> availablePackages)
        {
            return new PackageResolverContext(behavior,
                targets.Select(p => p.Id), Enumerable.Empty<string>(),
                Enumerable.Empty<PackageReference>(),
                targets,
                availablePackages,
                Enumerable.Empty<PackageSource>(),
                NullLogger.Instance);
        }
    }
}
