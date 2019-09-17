using Semverify.AssemblyLoaders;
using Semverify.SemverModel;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
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
                new Option("--output-api", "The path to output the generated API text files")
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
                }
            };

            command.Handler = CommandHandler.Create<string, string, List<DirectoryInfo>, List<DirectoryInfo>, List<DirectoryInfo>, DirectoryInfo, SemverChangeType?>(RunCompare);
            return await command.InvokeAsync(args);
        }

        static async Task<int> RunCompare(string assembly1, string assembly2, List<DirectoryInfo> a1deps, List<DirectoryInfo> a2deps, List<DirectoryInfo> deps, DirectoryInfo outputApi, SemverChangeType? expectedChangeType)
        {
            var frameworkAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string).Split(';');

            var assembly1Loader = AssemblyLoaderFactory.ResolveLoader(assembly1);
            var assembly1Options = await assembly1Loader.LoadAssembly(new AssemblyLoaderOptions
            {
                AssemblyName = assembly1,
                AssemblyDependencyPaths = a1deps?.Select(d => d?.FullName).ToArray() ?? new string[0],
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
                AssemblyDependencyPaths = a2deps?.Select(d => d?.FullName).ToArray() ?? new string[0],
                FrameworkAssemblies = frameworkAssemblies
            });

            return SemverComparer.Compare(assembly1Options, assembly2Options, outputApi?.FullName, expectedChangeType);
        }
    }
}
