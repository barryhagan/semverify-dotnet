using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Semverify.ApiModel
{
    internal class ApiFieldInfo : ApiMemberInfo
    {
        protected readonly FieldInfo FieldInfo;

        public ApiFieldInfo(MemberInfo memberInfo) : base(memberInfo)
        {
            if (!(memberInfo is FieldInfo field))
            {
                throw new ArgumentException("The memberInfo is not a field", nameof(memberInfo));
            }

            FieldInfo = field;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var mods = MemberInfo.DeclaringType.IsInterface ? new List<string>() : GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";
            var name = $"{modString}{FieldInfo.FieldType.ResolveQualifiedName(FieldInfo.GetReferenceNullability())} {GetLocalName()};";
            return $"{new string(' ', indentLevel * IndentSpaces)}{name}";
        }

        public override string GetAccessor()
        {
            return "";
        }

        public override string GetFullName()
        {
            return $"{FieldInfo.DeclaringType.ResolveQualifiedName()}.{GetLocalName()}";
        }

        public override string GetLocalName()
        {
            return FieldInfo.Name;
        }

        public override IList<string> GetModifiers()
        {
            var mods = new List<string>();

            if (FieldInfo.DeclaringType.IsInterface)
            {
                mods.Add("public");
                return mods;
            }

            mods.AddFirstIf(new[] {
                (condition: FieldInfo.IsFamilyAndAssembly, value: "private protected"),
                (condition: FieldInfo.IsFamilyOrAssembly, value: "protected internal"),
                (condition: FieldInfo.IsAssembly, value: "internal"),
                (condition: FieldInfo.IsFamily, value: "protected"),
                (condition: FieldInfo.IsPublic, value: "public"),
            });

            var isConst = FieldInfo.IsLiteral && !FieldInfo.IsInitOnly;
            if (isConst)
            {
                mods.Add("const");
            }
            else
            {
                mods.AddIf(FieldInfo.IsStatic, "static");
                mods.AddIf(HasNewModifier(FieldInfo.DeclaringType.BaseType, FieldInfo), "new");
                mods.AddIf(FieldInfo.IsInitOnly || FieldInfo.HasAttribute(typeof(ReadOnlyAttribute)), "readonly");
            }

            return mods;
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            return $"{modString}{FieldInfo.FieldType.ResolveQualifiedName(FieldInfo.GetReferenceNullability())} {GetFullName()};";
        }
    }
}
