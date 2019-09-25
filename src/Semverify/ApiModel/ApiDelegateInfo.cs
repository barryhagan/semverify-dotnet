using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Semverify.ApiModel
{
    internal class ApiDelegateInfo : ApiTypeInfo
    {
        private readonly MethodInfo invokeMethod;

        public ApiDelegateInfo(MemberInfo memberInfo, MethodInfo invokeMethod) : base(memberInfo)
        {
            this.invokeMethod = invokeMethod;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var parameters = ResolveParameters(invokeMethod.GetParameters());
            var parameterString = string.Join(", ", parameters);

            var returnType = invokeMethod.ReturnType.ResolveQualifiedName(ApiMethodInfo.GetReturnTypeNullability(invokeMethod));
            var name = TypeInfo.ResolveLocalName(TypeInfo.GetReferenceNullability());
            var sig = $"{modString}delegate {returnType} {name}{FormatGenericArgs(invokeMethod)}({parameterString});";

            return $"{new string(' ', indentLevel * IndentSpaces)}{sig}";
        }

        public override IList<string> GetModifiers()
        {
            var mods = GetTypeAccessModifiers();

            if (TypeInfo.IsSealed && TypeInfo.IsAbstract)
            {
                mods.Add("static");
            }
            else
            {
                mods.AddIf(TypeInfo.IsAbstract && !TypeInfo.IsInterface, "abstract");
            }

            return mods;
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var parameters = ResolveParameters(invokeMethod.GetParameters());
            var parameterString = string.Join(", ", parameters);

            var returnType = invokeMethod.ReturnType.ResolveQualifiedName(ApiMethodInfo.GetReturnTypeNullability(invokeMethod));
            var name = TypeInfo.ResolveQualifiedName();

            return $"{modString}delegate {returnType} {name}{FormatGenericArgs(invokeMethod)}({parameterString});";
        }

        public override IEnumerable<ApiMemberInfo> EnumerateMembers()
        {
            return new List<ApiMemberInfo>();
        }
    }
}
