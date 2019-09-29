using Semverify.AssemblyLoaders;
using Semverify.SemverModel;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Semverify
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(), Console.InputEncoding, false, 8192));
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var command = new RootCommand
            {
                new Argument<string>("assembly1")
                {
                    Description = "The path or nuget package of the first assembly to compare",
                    Arity = ArgumentArity.ExactlyOne
                },
                new Argument<string>("assembly2")
                                {
                    Description = "The path or nuget package of the second assembly to compare",
                    Arity = ArgumentArity.ZeroOrOne
                },
                new Option(new string[]{"--deps", "--common-deps"}, "The path to dependencies shared by both assemblies")
                {
                    Argument = new Argument<List<DirectoryInfo>>
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }
                },
                new Option(new string[]{"--a1-deps", "--assembly1-deps-path"}, "The path to dependencies of assembly 1")
                {
                    Argument = new Argument<List<DirectoryInfo>>
                    {
                        Arity = ArgumentArity.ZeroOrMore,
                    }
                },
                new Option(new string[]{"--a2-deps", "--assembly2-deps-path"}, "The path to dependencies of assembly 2")
                {
                    Argument = new Argument<List<DirectoryInfo>>
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }
                },
                new Option(new string[]{"--output-api" }, "The path to output the generated API text files")
                {
                    Argument = new Argument<DirectoryInfo>
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.ExistingOnly()
                },
                new Option(new string[] {"--expected-change-type" }, "The expected semver change type for this comparison")
                {
                    Argument = new Argument<SemverChangeType?>
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    }
                },
                new Option(new string[] {"--use-dependency-changes" }, "Use dependency semver changes to calculate the assembly change type")
                {
                    Argument = new Argument<bool>
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    }
                }
            };

            command.Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(RunCompare), BindingFlags.NonPublic | BindingFlags.Static), null);
            return await command.InvokeAsync(args);
        }

        static async Task<int> RunCompare(
            string assembly1,
            string assembly2,
            List<DirectoryInfo> a1deps,
            List<DirectoryInfo> a2deps,
            List<DirectoryInfo> deps,
            DirectoryInfo outputApi,
            SemverChangeType? expectedChangeType,
            bool useDependencyChanges)
        {
            var platformDelimiter = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
            var frameworkAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string).Split(platformDelimiter);
            var commonDeps = deps?.Select(d => d?.FullName).ToList() ?? new List<string>();

            var assembly1Loader = AssemblyLoaderFactory.ResolveLoader(assembly1);
            var assembly1Options = await assembly1Loader.LoadAssembly(new AssemblyLoaderOptions
            {
                AssemblyName = assembly1,
                AssemblyDependencyPaths = commonDeps.Concat(a1deps?.Select(d => d?.FullName) ?? new List<string>()).ToArray(),
                FrameworkAssemblies = frameworkAssemblies
            });

            if (string.IsNullOrWhiteSpace(assembly2))
            {
                return SemverComparer.WritePublicApi(assembly1Options, outputApi?.FullName ?? "");
            }

            var assembly2Loader = AssemblyLoaderFactory.ResolveLoader(assembly2);
            var assembly2Options = await assembly2Loader.LoadAssembly(new AssemblyLoaderOptions
            {
                AssemblyName = assembly2,
                AssemblyDependencyPaths = commonDeps.Concat(a2deps?.Select(d => d?.FullName) ?? new List<string>()).ToArray(),
                FrameworkAssemblies = frameworkAssemblies
            });

            return SemverComparer.Compare(assembly1Options, assembly2Options, outputApi?.FullName, expectedChangeType, useDependencyChanges);
        }
    }
}
