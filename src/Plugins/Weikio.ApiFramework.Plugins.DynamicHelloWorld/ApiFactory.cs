using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCompiler;

namespace Weikio.ApiFramework.Plugins.DynamicHelloWorld
{
    public static class ApiFactory
    {
        public static async Task<IEnumerable<Type>> Create()
        {
            var generator = new AssemblyGenerator();

            string assemblySourceCode;
            var filePath = Path.Combine(Path.GetDirectoryName(typeof(ApiFactory).Assembly.Location), "DynamicHelloWorld.txt");
            var content = await File.ReadAllTextAsync(filePath);

            using (var sourceWriter = new SourceWriter())
            {
                sourceWriter.Write(content);

                assemblySourceCode = sourceWriter.Code();
            }

            var assembly = generator.Generate(assemblySourceCode);

            return assembly.GetExportedTypes()
                .Where(x => !x.IsAbstract && x.Name.EndsWith("Api"))
                .ToList();
        }

        public static async Task<IEnumerable<Type>> CreateWithParameters(string postfix, int age, ComplexConfiguration complex)
        {
            var generator = new AssemblyGenerator();

            string assemblySourceCode;
            var filePath = Path.Combine(Path.GetDirectoryName(typeof(ApiFactory).Assembly.Location), "ParametersDynamicHelloWorld.txt");
            var content = await File.ReadAllTextAsync(filePath);

            content = content.Replace("##POSTFIX##", postfix);
            content = content.Replace("##AGE##", age.ToString());

            using (var sourceWriter = new SourceWriter())
            {
                sourceWriter.Write(content);

                assemblySourceCode = sourceWriter.Code();
            }

            var assembly = generator.Generate(assemblySourceCode);

            return assembly.GetExportedTypes()
                .Where(x => !x.IsAbstract && x.Name.EndsWith("Api"))
                .ToList();
        }
    }
}
