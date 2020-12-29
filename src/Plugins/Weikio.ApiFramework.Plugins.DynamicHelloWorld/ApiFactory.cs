using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Plugins.DynamicHelloWorld
{
    public static class ApiFactory
    {
        public static async Task<IEnumerable<Type>> Create(string endpointRoute)
        {
            var generator = new Weikio.TypeGenerator.CodeToAssemblyGenerator();

            string assemblySourceCode;
            var filePath = Path.Combine(Path.GetDirectoryName(typeof(ApiFactory).Assembly.Location), "DynamicHelloWorld.txt");
            var content = await File.ReadAllTextAsync(filePath);

            var sb = new StringBuilder();
            sb.Append(content);

            var assembly = generator.GenerateAssembly(sb.ToString());

            return assembly.GetExportedTypes()
                .Where(x => !x.IsAbstract && x.Name.EndsWith("Api"))
                .ToList();
        }

        public static async Task<IEnumerable<Type>> CreateWithParameters(string postfix, int age, ComplexConfiguration complex)
        {
            var generator = new Weikio.TypeGenerator.CodeToAssemblyGenerator();

            var filePath = Path.Combine(Path.GetDirectoryName(typeof(ApiFactory).Assembly.Location), "ParametersDynamicHelloWorld.txt");
            var content = await File.ReadAllTextAsync(filePath);

            content = content.Replace("##POSTFIX##", postfix);
            content = content.Replace("##AGE##", age.ToString());

            var sb = new StringBuilder();
            sb.Append(content);

            var assembly = generator.GenerateAssembly(sb.ToString());

            return assembly.GetExportedTypes()
                .Where(x => !x.IsAbstract && x.Name.EndsWith("Api"))
                .ToList();
        }
    }
}
