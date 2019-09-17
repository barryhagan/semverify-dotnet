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
            var eventType = addMethod.GetParameters().First().ParameterType.ResolveQualifiedName();
            return $"{new string(' ', indentLevel * IndentSpaces)}{modString}event {eventType} {GetLocalName()}{GetAccessor()}";
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
                (condition: (access & MethodAttributes.FamANDAssem) == MethodAttributes.FamANDAssem, value: "private protected"),
                (condition: (access & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem, value: "protected internal"),
                (condition: (access & MethodAttributes.Family) == MethodAttributes.Family, value: "protected"),
            });

            if ((access & MethodAttributes.Abstract) == MethodAttributes.Abstract)
            {
                mods.Add("abstract");
            }
            else
            {
                var overrideFlags = MethodAttributes.Virtual | MethodAttributes.HideBySig;

                if ((access & overrideFlags) == overrideFlags)
                {
                    mods.Add("override");
                }
                else if (HasNewModifier(MemberInfo.DeclaringType.BaseType, MemberInfo.Name))
                {
                    mods.Add("new");
                }
                else
                {
                    mods.AddIf(((access & MethodAttributes.Virtual) == MethodAttributes.Virtual && (access & MethodAttributes.Final) != MethodAttributes.Final), "virtual");
                }

                mods.AddIf((access & MethodAttributes.Static) == MethodAttributes.Static, "static");
            }

            return mods;
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var eventType = addMethod.GetParameters().First().ParameterType.ResolveQualifiedName();

            return $"{modString}event {eventType} {GetFullName()}{GetAccessor()}";
        }
    }
}
