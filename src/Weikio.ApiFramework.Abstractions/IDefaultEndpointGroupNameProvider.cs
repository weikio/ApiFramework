namespace Weikio.ApiFramework.Abstractions
{
    public interface IDefaultEndpointGroupNameProvider
    {
        string GetDefaultGroupName(Endpoint endpoint);
    }
}
