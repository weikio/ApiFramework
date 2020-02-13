namespace Weikio.ApiFramework.Extensions.ResponceCache
{
    public enum ResponseCacheConfigurationLevel
    {
        Undefined,
        Global, // Response cache is defined on Api Framework's level and affects all the endpoints
        Endpoint, // Response cache is defined on the Endpoint level and affects all the routes for the Endpoint
        Route // Response cache is defined on the Endpoint's route level and affects only this particular route
    }
}
