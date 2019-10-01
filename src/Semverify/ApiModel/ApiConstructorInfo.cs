using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Semverify.ApiModel
{
    internal class ApiConstructorInfo : ApiMemberInfo
    {
        private readonly ConstructorInfo ctor;

        public ApiConstructorInfo(MemberInfo memberInfo) : base(memberInfo)
        {
            if (!(memberInfo is ConstructorInfo constructor))
            {
                throw new ArgumentException("The memberInfo is not a constructor", nameof(memberInfo));
            }

            ctor = constructor;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var mods = GetModifiers(); ;
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var parameters = GetParameters();
            var parameterString = string.Join(", ", parameters);

            var accessorString = $" {GetAccessor()}";

            var name = $"{modString}{GetConstructorName()}({parameterString}){accessorString}";

            return $"{new string(' ', indentLevel * IndentSpaces)}{name}";
        }

        public override string GetAccessor()
        {
            return "{ }";
        }

        public override string GetFullName()
        {
            var parameters = GetParameters();
            var parameterString = string.Join(", ", parameters);

            return $"{ctor.DeclaringType.ResolveQualifiedName()}.{GetLocalName()}({parameterString})";
        }

        public override string GetLocalName()
        {
            return GetConstructorName();
        }

        public override IList<string> GetModifiers()
        {
            var mods = new List<string>();
            mods.AddFirstIf(new[]
            {
                (condition: ctor.IsFamilyAndAssembly, value: "private protected"),
                (condition: ctor.IsFamilyOrAssembly, value: "protected internal"),
                (condition: ctor.IsAssembly, value: "internal"),
                (condition: ctor.IsFamily, value: "protected"),
                (condition: ctor.IsPublic, value: "public"),
           });

            mods.AddIf(ctor.IsStatic, "static");
            return mods;
        }

        public IList<string> GetParameters()
        {
            return ResolveParameters(ctor.GetParameters());
        }

        public override string GetSignature()
        {
            var mods = GetModifiers(); ;
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var accessorString = $" {GetAccessor()}";

            return $"{modString}{GetFullName()}{accessorString}";
        }

        private string GetConstructorName()
        {
            var constructorName = ctor.DeclaringType.Name;
            var genericIndex = constructorName.IndexOf('`');
            if (genericIndex > 0)
            {
                constructorName = constructorName.Substring(0, genericIndex);
            }
            return constructorName;
        }
    }
}
