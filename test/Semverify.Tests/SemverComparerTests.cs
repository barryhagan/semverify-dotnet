using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace Semverify.Tests
{
    public class SemverComparerTests
    {
        [Fact]
        public void write_assembly_api()
        {
            var platformDelimiter = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
            var frameworkAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string).Split(platformDelimiter);

            var assemblyPath = typeof(SemverComparerTests).Assembly.Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            var reflectInput = new AssemblyReflectionInput
            {
                AssemblyPath = assemblyPath,
                AssemblyDependencies = Directory.GetFiles(assemblyDirectory, "*.dll").Concat(frameworkAssemblies).ToArray()
            };

            SemverComparer.WritePublicApi(reflectInput, assemblyDirectory);
        }
    }
}
