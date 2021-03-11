using System;

namespace Weikio.ApiFramework.Abstractions
{
    public class ApiNotFoundException : Exception
    {
        public string Name { get; }
        public Version Version { get; }

        public ApiNotFoundException(string name, Version version, string message) : base(message)
        {
            Name = name;
            Version = version;
        }
    }
}
