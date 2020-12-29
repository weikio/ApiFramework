using Xunit;

namespace ApiFramework.IntegrationTests
{
    [CollectionDefinition(nameof(NotThreadSafeResourceCollection), DisableParallelization = true)]
    public class NotThreadSafeResourceCollection { }
}
