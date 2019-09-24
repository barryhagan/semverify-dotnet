using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Semverify.ApiModel
{
    internal class ApiModuleInfo
    {
        private readonly Module module;

        public string Name { get => module.Name; }

        public ApiModuleInfo(Module mod)
        {
            module = mod;
        }

        public static IDictionary<string, ApiModuleInfo> GetModulesForAssembly(Assembly assembly)
        {
            return assembly.GetModules().OrderBy(m => m.Name).ToDictionary(m => m.Name, m => new ApiModuleInfo(m));
        }

        public string FormatForApiOutput(int indentLevel = 0)
        {
            var indentSize = indentLevel * ApiMemberInfo.IndentSpaces;

            var apiBuilder = new StringBuilder();
            foreach (var namespaceGroup in EnumerateTypes().GroupBy(t => t.Namespace).OrderBy(g => g.Key))
            {
                apiBuilder.AppendLine($"{new string(' ', indentSize)}namespace {namespaceGroup.Key}");
                apiBuilder.AppendLine($"{new string(' ', indentSize)}{{");
                foreach (var apiTypeInfo in namespaceGroup.OrderBy(t => t.GetLocalName()))
                {
                    apiBuilder.AppendLine(apiTypeInfo.FormatForApiOutput(indentLevel + 1));
                }
                apiBuilder.AppendLine($"{new string(' ', indentSize)}}}");
            }
            return apiBuilder.ToString();
        }

        public IEnumerable<ApiMemberInfo> EnumerateAllMembers()
        {
            foreach (var type in EnumerateTypes().OrderBy(t => t.Namespace))
            {
                yield return type;
                foreach (var nested in EnumerateTypeMembers(type))
                {
                    yield return nested;
                }
            }
        }

        public IEnumerable<ApiTypeInfo> EnumerateTypes()
        {
            foreach (var type in module.GetTypes().Where(t => t.IsVisible && !t.IsNested).OrderBy(t => t.FullName ?? t.Name))
            {
                yield return ApiTypeInfo.LoadType(type);
            }
        }

        private IEnumerable<ApiMemberInfo> EnumerateTypeMembers(ApiTypeInfo type)
        {
            var comparer = new MemberInfoDisplayComparer();
            foreach (var member in type.EnumerateMembers().OrderBy(m => m, comparer))
            {
                yield return member;
                if (member is ApiTypeInfo nestedType)
                {
                    foreach (var nested in EnumerateTypeMembers(nestedType))
                    {
                        yield return nested;
                    }
                }
            }
        }

    }
}
