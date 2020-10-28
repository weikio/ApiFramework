namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointRouteTemplateProvider
    {
        string GetRouteTemplate(Endpoint endpoint);
    }
}
