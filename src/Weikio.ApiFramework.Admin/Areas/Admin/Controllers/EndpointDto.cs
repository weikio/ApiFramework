using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    public class EndpointDto
    {
        public EndpointDto()
        {
        }

        public EndpointDto(Endpoint endpoint)
        {
            Route = endpoint.Route;
            Api = endpoint.Api.ApiDefinition;

            // TODO: Create these only once
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new MyContractResolver(),
            };

            Configuration = endpoint.Configuration == null ? null : Newtonsoft.Json.JsonConvert.SerializeObject(endpoint.Configuration, settings);
            EndpointStatus = endpoint.Status;
        }

        public string Route { get; }
        public ApiDefinition Api { get; }
        public string Configuration { get; }
        public EndpointStatus EndpointStatus { get; set; }
    }
    
    public class MyContractResolver:DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member,memberSerialization);

            var propertyType = property.PropertyType;

            if (typeof(MulticastDelegate).IsAssignableFrom(propertyType))
            {
                property.ShouldSerialize = instance => false;
            }
            else
            {
                property.ShouldSerialize = instance => true;
            }
            return property;
        }
    }
}
