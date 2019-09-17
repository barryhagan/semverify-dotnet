using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Semverify.AssemblyLoaders
{
    abstract internal class AssemblyLoader : IAssemblyLoader
    {
        abstract public Task<AssemblyReflectionInput> LoadAssembly(AssemblyLoaderOptions packageAlias);

        protected virtual IEnumerable<string> EnumerateAssemblies(string assemblyFileOrDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyFileOrDirectoryPath))
            {
                yield break;
            }

            if (File.Exists(assemblyFileOrDirectoryPath))
            {
                yield return assemblyFileOrDirectoryPath;
            }
            else if (Directory.Exists(assemblyFileOrDirectoryPath))
            {
                foreach (var dll in Directory.EnumerateFiles(assemblyFileOrDirectoryPath, "*.dll"))
                {
                    yield return dll;
                }
            }
        }
    }
}
