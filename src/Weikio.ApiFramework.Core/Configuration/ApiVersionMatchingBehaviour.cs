namespace Weikio.ApiFramework.Core.Configuration
{
    public enum ApiVersionMatchingBehaviour
    {
        OnlyExact,
        Lowest,
        HighestPatch,
        HighestMinor,
        Highest,
    }
}
