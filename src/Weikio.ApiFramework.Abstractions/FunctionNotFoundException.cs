using System;

namespace Weikio.ApiFramework.Abstractions
{
    public class FunctionNotFoundException : Exception
    {
        private readonly string _name;
        private readonly Version _version;

        public FunctionNotFoundException(string name, Version version)
        {
            _name = name;
            _version = version;
        }
    }
}
