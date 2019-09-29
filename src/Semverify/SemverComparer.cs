using Semverify.ApiModel;
using Semverify.SemverModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Semverify
{
    public static class SemverComparer
    {
        private static readonly List<char> InvalidCharsForFile = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToList();

        public static int WritePublicApi(AssemblyReflectionInput input, string outputApiPath)
        {
            var assembly1Paths = new[] { input.AssemblyPath }.Concat(input.AssemblyDependencies).Distinct();

            using (var loadContext1 = new MetadataLoadContext(new PathAssemblyResolver(assembly1Paths)))
            {
                var assembly1 = loadContext1.LoadFromAssemblyPath(input.AssemblyPath);
                var assembly1Semver = GetSemanticVersion(assembly1);
                var assembly1Modules = ApiModuleInfo.GetModulesForAssembly(assembly1);

                WriteApiToFile(assembly1Modules.Values, GetSafeApiFileName(outputApiPath, input.AssemblyPath, assembly1Semver, ".api.txt"));
                WriteSignaturesToFile(assembly1Modules.Values, GetSafeApiFileName(outputApiPath, input.AssemblyPath, assembly1Semver, ".signatures.txt"));
            }

            return 0;
        }

        public static int Compare(AssemblyReflectionInput assembly1Input, AssemblyReflectionInput assembly2Input, string outputApiPath = null, SemverChangeType? expectedChangeType = null, bool useDependencyChanges = false)
        {
            return CompareAction(assembly1Input, assembly2Input, (assembly1, assembly2) =>
            {
                var assembly1Semver = GetSemanticVersion(assembly1);
                var assembly2Semver = GetSemanticVersion(assembly2);

                var assembly1Modules = ApiModuleInfo.GetModulesForAssembly(assembly1);
                var assembly2Modules = ApiModuleInfo.GetModulesForAssembly(assembly2);

                var compareResult = ComparePublicApi(assembly1Modules, assembly2Modules);

                var dependencyCompareResult = CompareDependencies(assembly1, assembly2);

                if (useDependencyChanges && dependencyCompareResult > compareResult)
                {
                    compareResult = dependencyCompareResult;
                }

                if (!string.IsNullOrWhiteSpace(outputApiPath) && Directory.Exists(outputApiPath))
                {
                    var path1 = GetSafeApiFileName(outputApiPath, assembly1.GetName().Name, assembly1Semver, ".api.txt");
                    var path2 = GetSafeApiFileName(outputApiPath, assembly2.GetName().Name, assembly2Semver, ".api.txt");
                    if (path2.Equals(path1, StringComparison.OrdinalIgnoreCase))
                    {
                        path1 = Path.Join(Path.GetDirectoryName(path1), Path.GetFileNameWithoutExtension(path1), "(1)", Path.GetExtension(path1));
                        path2 = Path.Join(Path.GetDirectoryName(path2), Path.GetFileNameWithoutExtension(path2), "(2)", Path.GetExtension(path2));
                    }
                    WriteApiToFile(assembly1Modules.Values, path1);
                    WriteApiToFile(assembly2Modules.Values, path2);
                }

                var calculatedSemver = CalculateSemver(assembly1Semver, compareResult);

                if (expectedChangeType.HasValue)
                {
                    if (expectedChangeType >= compareResult || assembly1Semver.IsPrerelease)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine();
                        Console.WriteLine($"Valid [{expectedChangeType}] semver change for {Path.GetFileNameWithoutExtension(assembly1.GetName().Name)} ({assembly1Semver.ToShortVerion()} => {assembly2Semver.ToShortVerion()}). Calculated as [{compareResult}] ({calculatedSemver})");
                        Console.ResetColor();
                        return 0;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine($"Changes for {Path.GetFileNameWithoutExtension(assembly1.GetName().Name)} ({assembly1Semver.ToShortVerion()} => {assembly2Semver.ToShortVerion()}) do not meet semver guidelines for a [{expectedChangeType}] change.  Calculated as [{compareResult}] ({calculatedSemver})");
                        Console.ResetColor();
                        return 1;
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"The calculated semver for {Path.GetFileNameWithoutExtension(assembly1.GetName().Name)} ({assembly1Semver.ToShortVerion()} => {assembly2Semver.ToShortVerion()}) is [{compareResult}] ({calculatedSemver})");
                    return 0;
                }
            });
        }

        private static int CompareAction(AssemblyReflectionInput assembly1Input, AssemblyReflectionInput assembly2Input, Func<Assembly, Assembly, int> compareFunc)
        {
            var assembly1Paths = new[] { assembly1Input.AssemblyPath }.Concat(assembly1Input.AssemblyDependencies).Distinct();
            var assembly2Paths = new[] { assembly2Input.AssemblyPath }.Concat(assembly2Input.AssemblyDependencies).Distinct();

            using var loadContext1 = new MetadataLoadContext(new PathAssemblyResolver(assembly1Paths));
            using var loadContext2 = new MetadataLoadContext(new PathAssemblyResolver(assembly2Paths));

            var assembly1 = loadContext1.LoadFromAssemblyPath(assembly1Input.AssemblyPath);
            var assembly2 = loadContext2.LoadFromAssemblyPath(assembly2Input.AssemblyPath);

            return compareFunc(assembly1, assembly2);
        }

        private static SemverChangeType CompareDependencies(Assembly assembly1, Assembly assembly2)
        {
            var assembly1Dependencies = assembly1.GetReferencedAssemblies().ToDictionary(a => a.Name, a => a.Version);
            var assembly2Dependencies = assembly2.GetReferencedAssemblies().ToDictionary(a => a.Name, a => a.Version);

            var calculatedChange = SemverChangeType.None;
            foreach (var dependency in assembly2Dependencies)
            {
                if (!assembly1Dependencies.TryGetValue(dependency.Key, out var previousVersion))
                {
                    Console.WriteLine($"New dependency added: {dependency.Key} {dependency.Value}");
                }
                else if (previousVersion != dependency.Value)
                {
                    var change = SemverChangeType.None;
                    if (Semver.TryParse(previousVersion, out var previousSemver) && Semver.TryParse(dependency.Value, out var newSemver))
                    {
                        change = previousSemver.GetChangeType(newSemver);
                        if (change > calculatedChange)
                        {
                            calculatedChange = change;
                        }
                    }
                    Console.WriteLine($"Dependency version change: {dependency.Key} ({previousVersion} => {dependency.Value}) [{change}]");
                }
            }
            return calculatedChange;
        }

        private static SemverChangeType ComparePublicApi(IDictionary<string, ApiModuleInfo> assembly1Modules, IDictionary<string, ApiModuleInfo> assembly2Modules)
        {
            if (!assembly1Modules.Keys.OrderBy(k => k).SequenceEqual(assembly2Modules.Keys.OrderBy(k => k)))
            {
                Console.WriteLine();
                Console.WriteLine("The assemblies have different modules that will not be compared.");
            }

            var changes = new List<(ApiChangeType changeType, ApiMemberInfo member)>();

            foreach (var (modName, assembly1Module) in assembly1Modules)
            {
                if (!assembly2Modules.TryGetValue(modName, out var assembly2Module))
                {
                    continue;
                }

                var assembly2ModuleTypes = assembly2Module.EnumerateTypes().ToDictionary(t => t.GetSignature(), t => t);
                foreach (var (signature, assembly1Type) in assembly1Module.EnumerateTypes().ToDictionary(t => t.GetSignature(), t => t))
                {
                    if (assembly2ModuleTypes.TryGetValue(signature, out var assembly2Type))
                    {
                        CompareApiTypeInfos(assembly1Type, assembly2Type, changes);
                        assembly2ModuleTypes.Remove(signature);
                    }
                    else
                    {
                        changes.Add((ApiChangeType.Removal, assembly1Type));
                    }
                }

                foreach (var (signature, assembly2Type) in assembly2ModuleTypes)
                {
                    changes.Add((ApiChangeType.Addition, assembly2Type));
                }
            }

            return ProcessChanges(changes);
        }

        private static SemverChangeType ProcessChanges(IList<(ApiChangeType changeType, ApiMemberInfo member)> changes)
        {
            foreach (var changesByName in changes.GroupBy(c => (c.member.MemberType, c.member.GetFullName())))
            {
                var adds = changesByName.Where(c => c.changeType == ApiChangeType.Addition).OrderBy(c => c.member, new MemberInfoDisplayComparer()).ToList();
                var removes = changesByName.Where(c => c.changeType == ApiChangeType.Removal).OrderBy(c => c.member, new MemberInfoDisplayComparer()).ToList();

                var modifications = adds.Zip(removes, (a, r) => (changeType: ApiChangeType.Removal & ApiChangeType.Addition, members: new[] { r, a })).ToList();
                var modCount = modifications.Count;

                modifications.AddRange(adds.Skip(modCount).Select(a => (changeType: ApiChangeType.Addition, members: new[] { a })));
                modifications.AddRange(removes.Skip(modCount).Select(a => (changeType: ApiChangeType.Removal, members: new[] { a })));

                foreach (var (changeType, changePairs) in modifications.OrderBy(c => c.members.First().member, new MemberInfoDisplayComparer()))
                {                    
                    switch (changeType)
                    {
                        case ApiChangeType.Addition:
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;
                        case ApiChangeType.Removal:
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            break;
                        case ApiChangeType.Addition & ApiChangeType.Removal:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;
                    }

                    foreach (var change in changePairs)
                    {
                        var prefix = "";
                        switch (change.changeType)
                        {
                            case ApiChangeType.Addition:
                                prefix = "+ ";
                                break;
                            case ApiChangeType.Removal:
                                prefix = "- ";
                                break;
                        }
                        Console.WriteLine($"{prefix}{change.member.GetSignature()}");
                    }

                    Console.ResetColor();
                    Console.WriteLine();
                }
            }

            var semverChangeType = changes.Any(c => c.changeType == ApiChangeType.Removal) ? SemverChangeType.Major
                 : changes.Any(c => c.changeType == ApiChangeType.Addition) ? SemverChangeType.Minor
                 : SemverChangeType.Patch;
           
            return semverChangeType;
        }

        private static Semver CalculateSemver(Semver current, SemverChangeType compareResult)
        {
            var calculatedSemver = new Semver
            {
                Major = current.Major ?? 0,
                Minor = current.Minor ?? 0,
                Patch = current.Patch ?? 0
            };

            if (current.IsPrerelease)
            {
                calculatedSemver.Prerelease = current.Prerelease + ".NEXT";
            }
            else
            {
                switch (compareResult)
                {
                    case SemverChangeType.Major:
                        calculatedSemver.Major++;
                        calculatedSemver.Minor = 0;
                        calculatedSemver.Patch = 0;
                        break;
                    case SemverChangeType.Minor:
                        calculatedSemver.Minor++;
                        calculatedSemver.Patch = 0;
                        break;
                    case SemverChangeType.Patch:
                    case SemverChangeType.None:
                        calculatedSemver.Patch++;
                        break;
                }
            }

            return calculatedSemver;
        }

        private static void CompareApiTypeInfos(ApiTypeInfo assembly1Type, ApiTypeInfo assembly2Type, IList<(ApiChangeType change, ApiMemberInfo member)> changes)
        {
            var type2Members = assembly2Type.EnumerateMembers().ToDictionary(m => m.GetSignature(), m => m);

            foreach (var member in assembly1Type.EnumerateMembers())
            {
                var signature = member.GetSignature();
                if (!type2Members.TryGetValue(signature, out var assembly2TypeMember))
                {
                    changes.Add((ApiChangeType.Removal, member));
                }
                else
                {
                    type2Members.Remove(signature);
                    if (member is ApiTypeInfo nestedType1Member && assembly2TypeMember is ApiTypeInfo nestedType2Member)
                    {
                        CompareApiTypeInfos(nestedType1Member, nestedType2Member, changes);
                    }
                }
            }

            foreach (var (sig, member) in type2Members)
            {
                changes.Add((ApiChangeType.Addition, member));
            }
        }

        private static Semver GetSemanticVersion(Assembly assembly)
        {
            var informationVersionAttribute = assembly.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == typeof(AssemblyInformationalVersionAttribute).FullName);
            var informationalVersion = informationVersionAttribute?.ConstructorArguments.FirstOrDefault().Value?.ToString();

            if (Semver.TryParse(informationalVersion, out var semver))
            {
                return semver;
            }

            if (Semver.TryParse(assembly.GetName().Version, out semver))
            {
                return semver;
            }

            return null;
        }

        private static string GetSafeApiFileName(string outputPath, string assemblyPath, Semver semver, string suffix)
        {
            var name = $"{Path.GetFileNameWithoutExtension(assemblyPath)}_{semver}{suffix}";
            var safeName = new string(name.Select(c => InvalidCharsForFile.Contains(c) ? '_' : c).ToArray());
            return Path.Combine(outputPath, safeName);
        }

        private static void WriteApiToFile(IEnumerable<ApiModuleInfo> modules, string outputPath)
        {
            var apiBuilder = new StringBuilder();

            foreach (var mod in modules)
            {
                apiBuilder.AppendLine(mod.FormatForApiOutput());
            }

            File.WriteAllText(outputPath, apiBuilder.ToString());
        }

        private static void WriteSignaturesToFile(IEnumerable<ApiModuleInfo> modules, string outputPath)
        {
            var apiBuilder = new StringBuilder();

            foreach (var mod in modules)
            {
                foreach (var sig in mod.EnumerateAllMembers().Select(m => m.GetSignature()))
                {
                    apiBuilder.AppendLine($"[ApiSignature(\"{sig}\")]");
                }
            }

            File.WriteAllText(outputPath, apiBuilder.ToString());
        }
    }
}

