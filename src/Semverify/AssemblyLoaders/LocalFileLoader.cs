using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Semverify.AssemblyLoaders
{
    internal class LocalFileLoader : AssemblyLoader
    {
        public override Task<AssemblyReflectionInput> LoadAssembly(AssemblyLoaderOptions loadOptions)
        {
            var dependencies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var frameworkAssembly in loadOptions.FrameworkAssemblies)
            {
                AddUniqueAssemblies(dependencies, frameworkAssembly);
            }

            foreach (var dependencyPath in loadOptions.AssemblyDependencyPaths)
            {
                AddUniqueAssemblies(dependencies, dependencyPath);
            }

            if (!loadOptions.AssemblyDependencyPaths.Any())
            {
                AddUniqueAssemblies(dependencies, Path.GetDirectoryName(loadOptions.AssemblyName));
            }

            var assemblyInput = new AssemblyReflectionInput
            {
                AssemblyPath = loadOptions.AssemblyName,
                AssemblyDependencies = dependencies.Values.ToArray()
            };

            return Task.FromResult(assemblyInput);
        }
    }
}
