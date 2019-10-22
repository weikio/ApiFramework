using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCompiler;

namespace Weikio.ApiFramework.Plugins.Broken
{
    public static class ApiFactory
    {
        private static int _failureCount = 0;

        public static async Task<IEnumerable<Type>> Create()
        {
            _failureCount += 1;

            if (_failureCount < 6)
            {
                throw new Exception("Custom problem. Count: " + _failureCount);
            }

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
    }
}
