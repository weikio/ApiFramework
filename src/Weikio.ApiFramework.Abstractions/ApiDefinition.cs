using System;

namespace Weikio.ApiFramework.Abstractions
{
    public class ApiDefinition
    {
        public ApiDefinition(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; set; }
        public Version Version { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Version}";
        }
    }
}
