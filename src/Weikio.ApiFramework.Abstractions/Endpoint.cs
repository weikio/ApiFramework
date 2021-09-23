using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Abstractions
{
    public class Endpoint
    {
        public string Route { get; set; }
        public Api Api { get; }
        public object Configuration { get; private set; }
        public Func<Endpoint, Task<IHealthCheck>> HealthCheckFactory { get; }
        public List<Type> ApiTypes { get; private set; }
        public string GroupName { get; set; }
        public string Name { get;  set;}
        public string Description { get; }
        public string[] Tags { get; }
        public IHealthCheck HealthCheck { get; private set; }
        public EndpointStatus Status { get; }
        public List<object> Metadata { get; private set; } = new List<object>();
        public EndpointDefinition Definition { get; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return $"{Name} ({Route})";
            }

            return Route;
        }

        public bool HasHealthCheck
        {
            get
            {
                return HealthCheck != null;
            }
        }

        public Endpoint(EndpointDefinition definition, Api api) 
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            
            if (api == null)
            {
                throw new ArgumentNullException(nameof(api));
            }
            
            Route = definition.Route;
            Api = api;
            Configuration = definition.Configuration;
            HealthCheckFactory = definition.HealthCheckFactory;

            ApiTypes = new List<Type>();
            Status = new EndpointStatus();

            GroupName = definition.GroupName;
            Name = definition.Name;

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = definition.Route.Trim('/');
            }
            
            Description = definition.Description;
            Tags = definition.Tags;
            Definition = definition;
        }

        public Endpoint(string route, Api api, object configuration = null, Func<Endpoint, Task<IHealthCheck>> healthCheckFactory = null,
            string groupName = null, string name = null, string description = null, string[] tags = null, string policyName=null) : this(
            new EndpointDefinition(route, api.ApiDefinition, configuration, healthCheckFactory, groupName, name, description, tags, policyName), api)
        {
        }

        public async Task Initialize()
        {
            Status.UpdateStatus(EndpointStatusEnum.Initializing, "Initializing");

            try
            {
                var dynamicApis = await InitializeApi();
                ApiTypes.AddRange(dynamicApis);

                ApiTypes.AddRange(Api.ApiTypes);

                if (HealthCheckFactory != null)
                {
                    HealthCheck = await HealthCheckFactory(this);
                }

                Status.UpdateStatus(EndpointStatusEnum.Ready, "Ready");
            }
            catch (Exception e)
            {
                Status.UpdateStatus(EndpointStatusEnum.Failed, "Failed: " + e);
            }
        }

        public void AddMetadata(object metadata)
        {
            Metadata.Add(metadata);
        }
        

        private async Task<List<Type>> InitializeApi()
        {
            var result = new List<Type>();

            if (Api.Factory == null)
            {
                return result;
            }

            var task = Api.Factory(this); 
            var createdApis = await task;

            result.AddRange(createdApis);

            return result;
        }
    }
}
