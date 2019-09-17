using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Semverify.ApiModel
{
    internal class ApiFieldInfo : ApiMemberInfo
    {
        private readonly FieldInfo fieldInfo;

        public ApiFieldInfo(MemberInfo memberInfo) : base(memberInfo)
        {
            if (!(memberInfo is FieldInfo field))
            {
                throw new ArgumentException("The memberInfo is not a field", nameof(memberInfo));
            }

            fieldInfo = field;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var mods = MemberInfo.DeclaringType.IsInterface ? new List<string>() : GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";
            var name = $"{modString}{fieldInfo.FieldType.ResolveQualifiedName()} {GetLocalName()};";
            return $"{new string(' ', indentLevel * IndentSpaces)}{name}";
        }

        public override string GetAccessor()
        {
            return "";
        }

        public override string GetFullName()
        {
            return $"{fieldInfo.DeclaringType.ResolveQualifiedName()}.{GetLocalName()}";
        }

        public override string GetLocalName()
        {
            return fieldInfo.Name;
        }

        public override IList<string> GetModifiers()
        {
            var mods = new List<string>();

            if (fieldInfo.DeclaringType.IsInterface)
            {
                mods.Add("public");
                return mods;
            }

            mods.AddFirstIf(new[] {
                (condition: fieldInfo.IsFamilyAndAssembly, value: "private protected"),
                (condition: fieldInfo.IsFamilyOrAssembly, value: "protected internal"),
                (condition: fieldInfo.IsAssembly, value: "internal"),
                (condition: fieldInfo.IsFamily, value: "protected"),
                (condition: fieldInfo.IsPublic, value: "public"),
            });

            var isConst = fieldInfo.IsLiteral && !fieldInfo.IsInitOnly;
            if (isConst)
            {
                mods.Add("const");
            }
            else
            {
                mods.AddIf(HasNewModifier(fieldInfo.DeclaringType.BaseType, fieldInfo.Name), "new");
                mods.AddIf(fieldInfo.IsStatic, "static");
                mods.AddIf(fieldInfo.IsInitOnly || fieldInfo.HasAttribute(typeof(ReadOnlyAttribute)), "readonly");
            }

            return mods;
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            return $"{modString}{fieldInfo.FieldType.ResolveQualifiedName()} {GetFullName()};";
        }
    }
}
