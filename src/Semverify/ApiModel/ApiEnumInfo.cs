using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Semverify.ApiModel
{
    internal class ApiEnumInfo : ApiTypeInfo
    {
        public ApiEnumInfo(MemberInfo memberInfo) : base(memberInfo)
        {
        }

        public override IList<string> GetModifiers()
        {
            var mods = GetTypeAccessModifiers();
            mods.Add("enum");
            return mods;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var indentSize = indentLevel * IndentSpaces;

            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var baseType = (MemberInfo as TypeInfo).GetEnumUnderlyingType();
            var baseTypeString = baseType.FullName == typeof(int).FullName ? "" : $" : {baseType.ResolveQualifiedName()}";

            var name = $"{modString}{GetLocalName()}{baseTypeString}";

            var apiBuilder = new StringBuilder();
            apiBuilder.AppendLine($"{new string(' ', indentSize)}{name}");
            apiBuilder.AppendLine($"{new string(' ', indentSize)}{{");

            foreach (var enumValue in EnumerateMembers())
            {
                apiBuilder.AppendLine(enumValue.FormatForApiOutput(indentLevel + 1));
            }

            apiBuilder.AppendLine($"{new string(' ', indentSize)}}}");
            return apiBuilder.ToString();
        }

        public override IEnumerable<ApiMemberInfo> EnumerateMembers()
        {
            foreach (var enumValue in TypeInfo.GetFields(BindingFlags.Public | BindingFlags.Static).OrderBy(e => e.GetRawConstantValue()))
            {
                if (enumValue.IsSpecialName)
                {
                    continue;
                }
                yield return new ApiEnumFieldInfo(enumValue);
            }
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var baseType = (MemberInfo as TypeInfo).GetEnumUnderlyingType();
            var baseTypeString = $" : {baseType.ResolveQualifiedName()}";

            var accessorString = $" {GetAccessor()}";

            return $"{modString}{GetFullName()}{baseTypeString}{accessorString}";
        }
    }
}
