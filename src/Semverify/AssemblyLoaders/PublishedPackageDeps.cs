using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.AssemblyLoaders
{
    internal class PublishedPackageDeps
    {
        public Dictionary<string, Dictionary<string, PublishedPackageDepsTarget>> Targets { get; set; }
    }

    internal class PublishedPackageDepsTarget
    {
        public Dictionary<string, PublishedPackageDepsRuntime> Runtime { get; set; }
    }

    internal class PublishedPackageDepsRuntime
    {
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
    }
}
