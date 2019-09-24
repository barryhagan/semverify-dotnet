using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Semverify.ApiModel
{
    internal class ApiEnumFieldInfo : ApiFieldInfo
    {
        public ApiEnumFieldInfo(MemberInfo memberInfo) : base(memberInfo)
        {
        }

        public object GetRawConstantValue() => FieldInfo.GetRawConstantValue();

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            return $"{new string(' ', indentLevel * IndentSpaces)}{FieldInfo.Name} = {FieldInfo.GetRawConstantValue()},";
        }

        public override string GetAccessor()
        {
            return "";
        }

        public override IList<string> GetModifiers()
        {
            var mods = new List<string>();
            mods.AddFirstIf(new[]
            {
                (condition: FieldInfo.DeclaringType.IsNestedFamANDAssem, value: "private protected"),
                (condition: FieldInfo.DeclaringType.IsNestedFamORAssem, value: "protected internal"),
                (condition: FieldInfo.DeclaringType.IsNestedAssembly, value: "internal"),
                (condition: FieldInfo.DeclaringType.IsNestedFamily, value: "protected"),
                (condition: FieldInfo.DeclaringType.IsPublic || FieldInfo.DeclaringType.IsNestedPublic, value: "public"),
            });
            return mods;
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";
            return $"{modString}{GetFullName()} = {FieldInfo.GetRawConstantValue()};";
        }
    }
}
