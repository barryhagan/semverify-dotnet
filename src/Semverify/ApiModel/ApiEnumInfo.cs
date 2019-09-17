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
            var innerIndent = (indentLevel + 1) * IndentSpaces;

            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var name = $"{modString}{GetLocalName()}";

            var apiBuilder = new StringBuilder();
            apiBuilder.AppendLine($"{new string(' ', indentSize)}{name}");
            apiBuilder.AppendLine($"{new string(' ', indentSize)}{{");

            var enumValues = TypeInfo.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var enumValue in enumValues.OrderBy(e => e.GetRawConstantValue()))
            {
                if (enumValue.IsSpecialName)
                {
                    continue;
                }
                apiBuilder.AppendLine($"{new string(' ', innerIndent)}{enumValue.Name} = {enumValue.GetRawConstantValue()},");
            }

            apiBuilder.AppendLine($"{new string(' ', indentSize)}}}");
            return apiBuilder.ToString();
        }

        public override IEnumerable<ApiMemberInfo> EnumerateMembers()
        {
            return new List<ApiMemberInfo>();
        }
    }
}
