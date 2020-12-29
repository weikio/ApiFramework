using System;
using System.Text;

namespace Weikio.ApiFramework.Abstractions
{
    public class ApiDefinition
    {
        public ApiDefinition()
        {
        }

        public ApiDefinition(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
        public string ProductVersion { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Version} {GetMoreVersionDetails()}".Trim();
        }

        private string GetMoreVersionDetails()
        {
            if (string.IsNullOrWhiteSpace(Description) && string.IsNullOrWhiteSpace(ProductVersion))
            {
                return string.Empty;
            }

            var result = new StringBuilder("(");

            if (string.IsNullOrWhiteSpace(Description))
            {
                result.Append(ProductVersion);
            }
            else if (string.IsNullOrWhiteSpace(ProductVersion))
            {
                result.Append(Description);
            }
            else
            {
                result.Append($"{ProductVersion}, {Description}");
            }
            
            result.Append(")");

            return result.ToString();
        }
    }
}
