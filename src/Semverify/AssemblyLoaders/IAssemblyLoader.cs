using System.Threading.Tasks;

namespace Semverify.AssemblyLoaders
{
    internal interface IAssemblyLoader
    {
        Task<AssemblyReflectionInput> LoadAssembly(AssemblyLoaderOptions packageAlias);
    }
}
