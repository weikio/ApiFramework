namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointExtension
    {
        string Key { get; }
        object Data { get; set; }
    }
}