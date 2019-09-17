using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Semverify.AssemblyLoaders
{
    internal static class AssemblyLoaderFactory
    {
        public static IAssemblyLoader ResolveLoader(string packageAlias)
        {
            if (File.Exists(packageAlias))
            {
                return new LocalFileLoader();
            }
            else if (Regex.IsMatch(packageAlias, ".+[/@].+"))
            {
                return new NugetPackageLoader();
            }

            throw new ArgumentException($"Unable to determine the assembly to load for {packageAlias}.  Specify a path to a valid .DLL, or a NuGet package using the format <package>@<version>");
        }
    }
}
