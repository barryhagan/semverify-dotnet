using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Semverify.AssemblyLoaders
{
    internal class NugetPackageLoader : AssemblyLoader
    {
        private const string AssemblyCacheDirectoryName = "semverify_assembly_cache";
        private const int ProcessTimeout = 60000;
        private readonly string workingDirectory;

        public NugetPackageLoader()
        {
            workingDirectory = Path.Combine(Path.GetTempPath(), AssemblyCacheDirectoryName);

            if (!Directory.Exists(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
            }
        }

        public override async Task<AssemblyReflectionInput> LoadAssembly(AssemblyLoaderOptions loadOptions)
        {
            var parts = loadOptions.AssemblyName.Split('/', '@');
            if (parts.Length < 2)
            {
                throw new ArgumentException("A NuGet package version must be specified as <package>@<version> or <package>/<version>");
            }

            var packageName = parts[0];
            var packageVersion = parts[1];

            var packageDirectory = Path.Combine(workingDirectory, packageName);
            var versionDirectory = Path.Combine(packageDirectory, packageVersion);
            var projectDirectory = Path.Combine(versionDirectory, "Semverify");
            var depsPath = Path.Combine(projectDirectory, "Assemblies", $"Semverify.deps.json");


            if (!File.Exists(depsPath))
            {
                var packageMeta = await LoadPackageMetadata(packageName, packageVersion);

                Console.WriteLine($"Adding NuGet package {packageMeta.Identity} to the assembly cache.");

                if (!Directory.Exists(packageDirectory))
                {
                    Directory.CreateDirectory(packageDirectory);
                }

                if (!Directory.Exists(versionDirectory))
                {
                    Directory.CreateDirectory(versionDirectory);
                }

                if (!Directory.Exists(projectDirectory))
                {
                    RunDotnetProcess(versionDirectory, $"new classlib -n Semverify");
                    RunDotnetProcess(projectDirectory, $"add package {packageName}{(string.IsNullOrWhiteSpace(packageVersion) ? "" : $" -v {packageVersion}")}");
                    RunDotnetProcess(projectDirectory, "publish -c Release -o Assemblies");
                }
            }

            var packageDeps = JsonConvert.DeserializeObject<PublishedPackageDeps>(File.ReadAllText(depsPath));
            var assemblyNames = packageDeps.Targets.Values
                .SelectMany(v => v.Where(kvp => kvp.Key.StartsWith($"{packageName}/", StringComparison.OrdinalIgnoreCase))
                .SelectMany(a => a.Value.Runtime.Select(kvp => Path.GetFileName(kvp.Key))))
                .Distinct()
                .ToList();

            if (assemblyNames.Count == 1)
            {
                var dependencies = loadOptions.AssemblyDependencyPaths.SelectMany(p => EnumerateAssemblies(p)).ToList();
                dependencies.AddRange(EnumerateAssemblies(Path.Combine(projectDirectory, "Assemblies")));

                var assemblyInput = new AssemblyReflectionInput
                {
                    AssemblyPath = Path.Combine(projectDirectory, "Assemblies", assemblyNames.First()),
                    AssemblyDependencies = dependencies.Concat(loadOptions.FrameworkAssemblies).Distinct().ToArray()
                };

                return assemblyInput;
            }

            throw new ApplicationException($"Unable to determine the assembly name for {packageName}");
        }

        private async Task<IPackageSearchMetadata> LoadPackageMetadata(string packageName, string packageVersion)
        {
            var packageSpec = new PackageIdentity(packageName, NuGetVersion.Parse(packageVersion));
            var repositoryProvider = new SourceRepositoryProvider(Settings.LoadDefaultSettings(root: null), Repository.Provider.GetCoreV3());

            foreach (var sourceRepository in repositoryProvider.GetRepositories())
            {
                var resource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
                using (var tokenSource = new CancellationTokenSource())
                {
                    var packageMeta = await resource.GetMetadataAsync(packageSpec, new NuGet.Common.NullLogger(), tokenSource.Token);
                    if (packageMeta != null)
                    {
                        return packageMeta;
                    }
                }
            }

            throw new ArgumentException($"Unable to find {packageName}@{packageVersion} from the configured NuGet repositories.");
        }

        private void RunDotnetProcess(string runInDirectory, string processArgs)
        {
            var output = new StringBuilder();

            using (var dotnetNewProcess = new System.Diagnostics.Process()
            {
                StartInfo =
                    {
                        CreateNoWindow = true,
                        FileName = "dotnet",
                        Arguments = processArgs,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = runInDirectory
                    }
            })
            {
                dotnetNewProcess.OutputDataReceived += (s, args) =>
                {
                    if (args?.Data == null) return;
                    output.Append(args.Data);
                };

                dotnetNewProcess.ErrorDataReceived += (s, args) =>
                {
                    if (args?.Data == null) return;
                    output.Append(args.Data);
                };

                dotnetNewProcess.Start();
                dotnetNewProcess.BeginOutputReadLine();
                dotnetNewProcess.BeginErrorReadLine();
                if (dotnetNewProcess.WaitForExit(ProcessTimeout))
                {
                    var exitCode = dotnetNewProcess.ExitCode;
                    if (exitCode != 0)
                    {
                        dotnetNewProcess.WaitForExit();  //ensure all message output has been captured from the async handlers.
                        throw new ApplicationException(
                            $"Command failed: 'dotnet {processArgs}'. Exit Code: {exitCode} Output: {output}");
                    }
                }
                else
                {
                    dotnetNewProcess.Kill();
                    throw new ApplicationException($"The dotnet process took more than {TimeSpan.FromMilliseconds(ProcessTimeout).TotalSeconds} seconds and has been killed.");
                }
            }
        }

    }
}
