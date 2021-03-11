using System;

namespace Weikio.ApiFramework.Abstractions
{
    public class PluginForApiNotFoundException : Exception
    {
        private readonly string _name;
        private readonly Version _version;

        public PluginForApiNotFoundException(string name, Version version)
        {
            _name = name;
            _version = version;
        }
    }
}