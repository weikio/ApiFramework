namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointDefinitionExtension
    {
        string Key { get; set; }
        object Data { get; set; }
    }
}
