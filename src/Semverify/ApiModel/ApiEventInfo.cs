using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Semverify.ApiModel
{
    internal class ApiEventInfo : ApiMemberInfo
    {
        private readonly MethodInfo addMethod;
        private readonly MethodInfo removeMethod;

        public ApiEventInfo(MemberInfo memberInfo) : base(memberInfo)
        {
            addMethod = memberInfo.GetType().GetProperty("AddMethod", BindingFlags.Instance | BindingFlags.Public).GetValue(memberInfo, null) as MethodInfo;
            removeMethod = memberInfo.GetType().GetProperty("RemoveMethod", BindingFlags.Instance | BindingFlags.Public).GetValue(memberInfo, null) as MethodInfo;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var mods = MemberInfo.DeclaringType.IsInterface ? new List<string>() : GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";
            var eventType = addMethod.GetParameters().First();
            var eventTypeString = eventType.ParameterType.ResolveQualifiedName(eventType.GetReferenceNullability(MemberInfo));
            return $"{new string(' ', indentLevel * IndentSpaces)}{modString}event {eventTypeString} {GetLocalName()}{GetAccessor()}";
        }

        public override string GetAccessor()
        {
            return ";";
        }

        public override string GetFullName()
        {
            return $"{MemberInfo.DeclaringType.ResolveQualifiedName()}.{GetLocalName()}";
        }

        public override string GetLocalName()
        {
            return MemberInfo.Name;
        }

        public override IList<string> GetModifiers()
        {
            var mods = new List<string>();

            if (MemberInfo.DeclaringType.IsInterface)
            {
                mods.Add("public");
                return mods;
            }

            var access = (addMethod.Attributes) | (removeMethod.Attributes);

            mods.AddFirstIf(new[]
            {
                (condition: (access & MethodAttributes.Public) == MethodAttributes.Public, value: "public"),
                (condition: (access & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem, value: "protected internal"),
                (condition: (access & MethodAttributes.Family) == MethodAttributes.Family, value: "protected"),
                (condition: (access & MethodAttributes.FamANDAssem) == MethodAttributes.FamANDAssem, value: "private protected"),
            });

            if ((access & MethodAttributes.Final) != MethodAttributes.Final)
            {
                mods.AddFirstIf(new[]
                {
                    ((access & MethodAttributes.Abstract) == MethodAttributes.Abstract, "abstract"),
                    ((access & ApiMethodInfo.IsVirtualFlags) == ApiMethodInfo.IsVirtualFlags, "virtual"),
                    ((access & ApiMethodInfo.IsOverrideFlags) == ApiMethodInfo.IsOverrideFlags, "override"),
                    (HasNewModifier(MemberInfo.DeclaringType.BaseType, MemberInfo), "new"),
                });
            }

            mods.AddIf((access & MethodAttributes.Static) == MethodAttributes.Static, "static");

            return mods;
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var eventParam = addMethod.GetParameters().First();
            var eventType = eventParam.ParameterType.ResolveQualifiedName(eventParam.GetReferenceNullability(addMethod));

            return $"{modString}event {eventType} {GetFullName()}{GetAccessor()}";
        }
    }
}
