namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointGroupNameProvider
    {
        string GetGroupName(Endpoint endpoint);
    }
}
