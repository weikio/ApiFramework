using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IFunctionProvider
    {
        Task Initialize();
        bool IsInitialized { get; }

        Task<List<FunctionDefinition>> List();

        Task<Function> Get(FunctionDefinition definition);
        Task<Function> Get(string name, Version version);
        Task<Function> Get(string name);
    }
}
