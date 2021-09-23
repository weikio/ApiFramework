namespace Weikio.ApiFramework.Abstractions
{
    public enum EndpointStatusEnum
    {
        New,
        Initializing,
        InitializingFailed,
        Ready,
        Changed,
        Failed,
        Unhealthy
    }
}
