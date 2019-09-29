using Semverify.AssemblyLoaders;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Semverify.Tests
{
    public class SemverComparerTests
    {
        [Fact]
        public async Task write_assembly_api()
        {
            var platformDelimiter = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
            var frameworkAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string).Split(platformDelimiter);

            var assemblyPath = typeof(SemverComparerTests).Assembly.Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);

            var loader = new LocalFileLoader();
            var reflectInput = await loader.LoadAssembly(new AssemblyLoaderOptions
            {
                AssemblyName = typeof(SemverComparerTests).Assembly.Location,
                FrameworkAssemblies = frameworkAssemblies
            });

            SemverComparer.WritePublicApi(reflectInput, assemblyDirectory);
        }
    }
}
