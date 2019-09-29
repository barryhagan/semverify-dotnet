using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.AssemblyLoaders
{
    internal class AssemblyLoaderOptions
    {
        public string AssemblyName { get; set; }
        public string[] AssemblyDependencyPaths { get; set; } = new string[0];
        public string[] FrameworkAssemblies { get; set; } = new string[0];
    }
}
