using System;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ConfigureOptionsWithDependency<TOptions, TDependency1>
    {
        public Action<TOptions, TDependency1> Action { get; set; }
    }
}
