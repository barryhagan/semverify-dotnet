using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify
{
    public class AssemblyReflectionInput
    {
        public string AssemblyPath { get; set; }
        public string[] AssemblyDependencies { get; set; }
    }
}
