namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiConfigurationTypeProvider
    {
        ApiConfiguration GetByApi(ApiDefinition apiDefinition);
        void Add(ApiConfiguration apiConfiguration);
    }
}
