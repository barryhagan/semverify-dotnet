using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semverify.AssemblyLoaders
{
    internal class LocalFileLoader : AssemblyLoader
    {
        public override Task<AssemblyReflectionInput> LoadAssembly(AssemblyLoaderOptions loadOptions)
        {
            var dependencies = loadOptions.AssemblyDependencyPaths.SelectMany(p => EnumerateAssemblies(p)).ToList();

            if (loadOptions.AssemblyDependencyPaths.All(p => string.IsNullOrWhiteSpace(p)))
            {
                dependencies.AddRange(EnumerateAssemblies(Path.GetDirectoryName(loadOptions.AssemblyName)));
            }

            var assemblyInput = new AssemblyReflectionInput
            {
                AssemblyPath = loadOptions.AssemblyName,
                AssemblyDependencies = dependencies.Concat(loadOptions.FrameworkAssemblies).Distinct().ToArray()
            };

            return Task.FromResult(assemblyInput);
        }
    }
}
