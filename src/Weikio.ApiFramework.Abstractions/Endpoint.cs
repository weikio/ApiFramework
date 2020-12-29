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

        public Endpoint(EndpointDefinition definition, Api api, Func<Endpoint, Task<IHealthCheck>> healthCheckFactory = null) : this(definition.Route, api,
            definition.Configuration, healthCheckFactory, definition.GroupName, definition.Name, definition.Description, definition.Tags)
        {
        }

        public Endpoint(string route, Api api, object configuration = null, Func<Endpoint, Task<IHealthCheck>> healthCheckFactory = null,
            string groupName = null, string name = null, string description = null, string[] tags = null)
        {
            Route = route;
            Api = api;
            Configuration = configuration;
            HealthCheckFactory = healthCheckFactory;

            ApiTypes = new List<Type>();
            Status = new EndpointStatus();

            GroupName = groupName;
            Name = name;

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = route.Trim('/');
            }
            
            Description = description;
            Tags = tags;
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
