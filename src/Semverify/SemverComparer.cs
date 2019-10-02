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

                WriteApiToFile(assembly1, GetSafeApiFileName(outputApiPath, input.AssemblyPath, assembly1Semver, ".api.txt"));
                WriteSignaturesToFile(assembly1, GetSafeApiFileName(outputApiPath, input.AssemblyPath, assembly1Semver, ".signatures.txt"));
            }

            return 0;
        }

        public static int Compare(AssemblyReflectionInput assembly1Input, AssemblyReflectionInput assembly2Input, string outputApiPath = null, SemverChangeType? expectedChangeType = null, bool useDependencyChanges = false)
        {
            return CompareAction(assembly1Input, assembly2Input, (assembly1, assembly2) =>
            {
                var assembly1Semver = GetSemanticVersion(assembly1);
                var assembly2Semver = GetSemanticVersion(assembly2);

                var compareResult = ComparePublicApi(assembly1, assembly2);

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
                    WriteApiToFile(assembly1, path1);
                    WriteApiToFile(assembly2, path2);
                }

                var calculatedSemver = CalculateSemver(assembly1Semver, compareResult);

                if (expectedChangeType.HasValue)
                {
                    if (expectedChangeType >= compareResult || assembly1Semver.IsPrerelease)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine();
                        Console.WriteLine($"Valid [{expectedChangeType}] semver change for {assembly1.GetName().Name} {assembly1Semver.ToShortVerion()} => {assembly2.GetName().Name} {assembly2Semver.ToShortVerion()}. Calculated as [{compareResult}] ({calculatedSemver})");
                        Console.ResetColor();
                        return 0;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine($"Changes for {assembly1.GetName().Name} {assembly1Semver.ToShortVerion()} => {assembly2.GetName().Name} {assembly2Semver.ToShortVerion()} do not meet semver guidelines for a [{expectedChangeType}] change.  Calculated as [{compareResult}] ({calculatedSemver})");
                        Console.ResetColor();
                        return 1;
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"The calculated semver for {assembly1.GetName().Name} {assembly1Semver.ToShortVerion()} => {assembly2.GetName().Name} {assembly2Semver.ToShortVerion()} is [{compareResult}] ({calculatedSemver})");
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

        private static SemverChangeType ComparePublicApi(Assembly assembly1, Assembly assembly2)
        {
            var changes = new List<(ApiChangeType changeType, ApiMemberInfo member)>();
            var unmatchedModules = new List<(string moduleName, string assemblyName)>();
            var assembly2Modules = assembly2.Modules.ToDictionary(m => m.Name, m => new ApiModuleInfo(m));

            foreach (var assembly1Module in assembly1.Modules.Select(m=>new ApiModuleInfo(m)))
            {
                if (!assembly2Modules.TryGetValue(assembly1Module.Name, out var assembly2Module))
                {
                    unmatchedModules.Add((assembly1Module.Name, assembly1.GetName().Name));
                    continue;
                }

                assembly2Modules.Remove(assembly2Module.Name);

                var assembly2ModuleTypes = assembly2Module.EnumerateTypes().ToDictionary(t => t.GetFullName(), t => t);
                foreach (var assembly1Type in assembly1Module.EnumerateTypes())
                {
                    var fullName = assembly1Type.GetFullName();

                    if (assembly2ModuleTypes.TryGetValue(fullName, out var assembly2Type))
                    {
                        CompareApiTypeInfos(assembly1Type, assembly2Type, changes);
                        assembly2ModuleTypes.Remove(fullName);
                    }
                    else
                    {
                        changes.Add((ApiChangeType.Removal, assembly1Type));
                    }
                }

                foreach (var assembly2Type in assembly2ModuleTypes.Values)
                {
                    changes.Add((ApiChangeType.Addition, assembly2Type));
                }
            }

            foreach (var assembly2Module in assembly2Modules.Values)
            {
                unmatchedModules.Add((assembly2Module.Name, assembly2.GetName().Name));
            }

            if (unmatchedModules.Any())
            {
                unmatchedModules.ForEach(m => Console.WriteLine($"Unmatched module: {m.moduleName} in {m.assemblyName}"));
                return SemverChangeType.Major;
            }

            return ProcessChanges(changes);
        }

        private static SemverChangeType ProcessChanges(IList<(ApiChangeType changeType, ApiMemberInfo member)> changes)
        {
            var effectiveChange = SemverChangeType.None;

            foreach (var changesByName in changes.GroupBy(c => (c.member.MemberType, c.member.Namespace, c.member.GetLocalName())))
            {
                var adds = changesByName.Where(c => c.changeType == ApiChangeType.Addition).ToDictionary(c => c.member.GetSignature(), c => c.member);
                var removes = changesByName.Where(c => c.changeType == ApiChangeType.Removal).ToDictionary(c => c.member.GetSignature(), c => c.member);
                var levenshteinPairs = new List<(int distance, string addKey, string removeKey)>();

                foreach (var add in adds)
                {
                    var levenshtein = new Levenshtein(add.Key);
                    foreach (var remove in removes)
                    {
                        var distance = levenshtein.DistanceFrom(remove.Key);
                        levenshteinPairs.Add((distance, add.Key, remove.Key));
                    }
                }

                var modifications = new List<(ApiChangeType modification, ApiMemberInfo memberAdded, ApiMemberInfo memberRemoved)>();

                foreach (var (distance, addKey, removeKey) in levenshteinPairs.OrderBy(lp => lp.distance).ThenBy(lp => lp.removeKey))
                {
                    if (adds.TryGetValue(addKey, out var add) && removes.TryGetValue(removeKey, out var remove))
                    {
                        modifications.Add((ApiChangeType.Addition | ApiChangeType.Removal, add, remove));
                        adds.Remove(addKey);
                        removes.Remove(removeKey);
                    }
                }

                modifications.AddRange(adds.Values.Select(a => (ApiChangeType.Addition, a, (ApiMemberInfo)null)));
                modifications.AddRange(removes.Values.Select(r => (ApiChangeType.Removal, (ApiMemberInfo)null, r)));

                var ruleEngine = new SemverRuleEngine();

                foreach (var (modification, added, removed) in modifications.OrderBy(c => c.memberAdded ?? c.memberRemoved, new MemberInfoDisplayComparer()))
                {                                        
                    switch (modification)
                    {
                        case ApiChangeType.Addition:
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;
                        case ApiChangeType.Removal:
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            break;
                        case ApiChangeType.Addition | ApiChangeType.Removal:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;
                    }

                    foreach (var rule in ruleEngine.InspectRules(removed, added))
                    {
                        if (effectiveChange < rule.ChangeType)
                        {
                            effectiveChange = rule.ChangeType;
                        }
                        Console.WriteLine($"[{rule.ChangeType}] {rule.RuleName}");
                    }

                    if (removed != null)
                    {
                        Console.WriteLine($"- {removed.GetSignature()}");
                    }

                    if (added != null)
                    {
                        Console.WriteLine($"+ {added.GetSignature()}");
                    }

                    Console.ResetColor();
                    Console.WriteLine();
                }
            }

            return effectiveChange;
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
                        if (calculatedSemver.Major == 0)
                        {
                            calculatedSemver.Minor++;
                            calculatedSemver.Patch = 0;
                        }
                        else
                        {
                            calculatedSemver.Major++;
                            calculatedSemver.Minor = 0;
                            calculatedSemver.Patch = 0;
                        }
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

            if (assembly1Type.GetSignature() != assembly2Type.GetSignature())
            {
                changes.Add((ApiChangeType.Removal, assembly1Type));
                changes.Add((ApiChangeType.Addition, assembly2Type));
            }

            var type2Members = assembly2Type.EnumerateMembers().ToDictionary(m => m.GetFullName(), m => m);

            foreach (var member in assembly1Type.EnumerateMembers())
            {
                var fullName = member.GetFullName();
                if (!type2Members.TryGetValue(fullName, out var assembly2TypeMember))
                {
                    changes.Add((ApiChangeType.Removal, member));
                }
                else
                {
                    if (member is ApiTypeInfo nestedType1Member && assembly2TypeMember is ApiTypeInfo nestedType2Member)
                    {
                        CompareApiTypeInfos(nestedType1Member, nestedType2Member, changes);
                    }
                    else if (member.GetSignature() != assembly2TypeMember.GetSignature())
                    {
                        changes.Add((ApiChangeType.Removal, member));
                        changes.Add((ApiChangeType.Addition, assembly2TypeMember));
                    }
                    type2Members.Remove(fullName);
                }
            }

            foreach (var member in type2Members.Values)
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

        private static void WriteApiToFile(Assembly assembly, string outputPath)
        {
            var apiBuilder = new StringBuilder();

            foreach (var mod in assembly.Modules.Select(m=>new ApiModuleInfo(m)))
            {
                apiBuilder.AppendLine(mod.FormatForApiOutput());
            }

            File.WriteAllText(outputPath, apiBuilder.ToString());
        }

        private static void WriteSignaturesToFile(Assembly assembly, string outputPath)
        {
            var apiBuilder = new StringBuilder();

            foreach (var mod in assembly.Modules.Select(m => new ApiModuleInfo(m)))
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

