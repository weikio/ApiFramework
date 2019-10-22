using Microsoft.Extensions.Options;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ConfigureOptionsWithDependencyContainer<TOptions, TDependency1> : IConfigureOptions<TOptions>
        where TOptions : class, new()
    {
        public ConfigureOptionsWithDependencyContainer(TDependency1 dependency1,
            IOptions<ConfigureOptionsWithDependency<TOptions, TDependency1>> configureOptionsWithDependency)
        {
            ConfigureOptionsWithDependency = configureOptionsWithDependency.Value;

            Dependency1 = dependency1;
        }

        public ConfigureOptionsWithDependency<TOptions, TDependency1> ConfigureOptionsWithDependency { get; set; }

        public TDependency1 Dependency1 { get; set; }

        public void Configure(TOptions options)
        {
            ConfigureOptionsWithDependency.Action(options, Dependency1);
        }
    }
}
