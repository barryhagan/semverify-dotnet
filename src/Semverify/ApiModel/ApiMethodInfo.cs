using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Semverify.ApiModel
{
    internal class ApiMethodInfo : ApiMemberInfo
    {
        internal const MethodAttributes IsVirtualFlags = MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
        internal const MethodAttributes IsOverrideFlags = MethodAttributes.Virtual | MethodAttributes.HideBySig;
        private static readonly Dictionary<string, string> OperatorAliases = new Dictionary<string, string>
        {
          {"op_Addition", "operator +"},
          {"op_BitwiseAnd", "operator &"},
          {"op_BitwiseOr", "operator |"},
          {"op_Decrement", "operator --"},
          {"op_Division", "operator /"},
          {"op_Equality", "operator =="},
          {"op_Explicit", "explicit operator "},
          {"op_ExclusiveOr", "operator ^"},
          {"op_False", "operator false"},
          {"op_GreaterThan", "operator >"},
          {"op_GreaterThanOrEqual", "operator >="},
          {"op_Implicit", "implicit operator "},
          {"op_Increment", "operator ++"},
          {"op_Inequality", "operator !="},
          {"op_LeftShift", "operator <<"},
          {"op_LessThan", "operator <"},
          {"op_LessThanOrEqual", "operator <="},
          {"op_LogicalNot", "operator !"},
          {"op_Modulus", "operator %"},
          {"op_Multiply", "operator *"},
          {"op_OnesComplement", "operator ~"},
          {"op_RightShift", "operator >>"},
          {"op_Subtraction", "operator -"},
          {"op_True", "operator true"},
          {"op_UnaryPlus", "operator +"},
        };

        protected readonly MethodInfo MethodInfo;

        public ApiMethodInfo(MemberInfo memberInfo) : base(memberInfo)
        {
            if (!(memberInfo is MethodInfo method))
            {
                throw new ArgumentException("The memberInfo is not a method", nameof(memberInfo));
            }

            MethodInfo = method;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var mods = MemberInfo.DeclaringType.IsInterface ? new List<string>() : GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var constraints = ResolveConstraints(MethodInfo.GetGenericArguments());
            var constraintString = constraints.Any() ? $" {string.Join(" ", constraints)}" : "";

            var extensionMethod = MethodInfo.HasAttribute(typeof(ExtensionAttribute)) ? "this " : "";

            var parameters = GetParameters();
            var parameterString = string.Join(", ", parameters);

            var accessor = GetAccessor();
            var accessorString = accessor.Length > 0 ? $" {accessor}" : ";";

            var returnParam = GetReturnParameter();

            if (MethodInfo.IsSpecialName && (MethodInfo.Name == "op_Implicit" || MethodInfo.Name == "op_Explicit"))
            {
                var name = $"{modString}{GetLocalName()}{returnParam.Type.ResolveLocalName(returnParam.ReferenceNullability, false)}({extensionMethod}{parameterString}){constraintString}{accessorString}";
                return $"{new string(' ', indentLevel * IndentSpaces)}{name}";
            }
            else
            {
                var name = $"{modString}{returnParam.Type.ResolveQualifiedName(returnParam.ReferenceNullability, false)} {GetLocalName()}({extensionMethod}{parameterString}){constraintString}{accessorString}";
                return $"{new string(' ', indentLevel * IndentSpaces)}{name}";
            }
        }

        public override string GetAccessor()
        {
            return MethodInfo.IsAbstract ? "" : "{ }";
        }

        public override string GetFullName()
        {
            var extensionMethod = MethodInfo.HasAttribute(typeof(ExtensionAttribute)) ? "this " : "";

            var parameters = GetParameters();
            var parameterString = string.Join(", ", parameters);

            return $"{MethodInfo.DeclaringType.ResolveQualifiedName()}.{MethodInfo.Name}{FormatGenericArgs()}({extensionMethod}{parameterString})";
        }

        public override string GetLocalName()
        {
            if (MethodInfo.IsSpecialName)
            {
                if (OperatorAliases.TryGetValue(MethodInfo.Name, out var op))
                {
                    return op;
                }
            }

            return $"{MethodInfo.Name}{FormatGenericArgs()}";
        }

        public override IList<string> GetModifiers()
        {
            var mods = new List<string>();

            if (MethodInfo.DeclaringType.IsInterface)
            {
                mods.Add("public");
                return mods;
            }

            mods.AddFirstIf(new[]
            {
                (condition: MethodInfo.IsFamilyAndAssembly, value: "private protected"),
                (condition: MethodInfo.IsFamilyOrAssembly, value: "protected internal"),
                (condition: MethodInfo.IsAssembly, value: "internal"),
                (condition: MethodInfo.IsFamily, value: "protected"),
                (condition: MethodInfo.IsPublic, value: "public"),
            });

            if (!MethodInfo.IsFinal)
            {
                mods.AddFirstIf(new[]
                {
                    (MethodInfo.IsAbstract, "abstract"),
                    ((MethodInfo.Attributes & IsVirtualFlags) == IsVirtualFlags, "virtual"),
                    ((MethodInfo.Attributes & IsOverrideFlags) == IsOverrideFlags, "override"),
                    (HasNewModifier(MethodInfo.DeclaringType.BaseType, MethodInfo), "new"),
                });
            }

            mods.AddIf(MethodInfo.IsStatic, "static");

            mods.AddIf(MethodInfo.HasAttribute(typeof(IsReadOnlyAttribute)) && !MethodInfo.HasAttribute(typeof(CompilerGeneratedAttribute)), "readonly");

            return mods;
        }

        public IEnumerable<ApiTypeDetails> GetParameters()
        {
            return ResolveParameters(MethodInfo.GetParameters(), !MethodInfo.DeclaringType.IsInterface);
        }

        public ApiTypeDetails GetReturnParameter()
        {
            return new ApiTypeDetails(MethodInfo.ReturnType, GetReturnTypeNullability(MethodInfo));
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var constraints = ResolveConstraints(MethodInfo.GetGenericArguments());
            var constraintString = constraints.Any() ? $" {string.Join(" ", constraints)}" : "";

            var accessor = GetAccessor();
            var accessorString = accessor.Length > 0 ? $" {accessor}" : ";";

            var returnParam = GetReturnParameter();

            return $"{modString}{returnParam.Type.ResolveQualifiedName(returnParam.ReferenceNullability, false)} {GetFullName()}{constraintString}{accessorString}";
        }

        protected virtual string FormatGenericArgs()
        {
            var genericArgs = MethodInfo.GetGenericArguments();
            if (genericArgs.Any())
            {
                return $"<{string.Join(", ", genericArgs.OrderBy(a => a.GenericParameterPosition).Select(a => a.ResolveLocalName(applyGenericModifiers: !MethodInfo.DeclaringType.IsInterface)))}>";
            }
            return string.Empty;
        }

        public static byte[] GetReturnTypeNullability(MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType.IsValueType)
            {
                return null;
            }

            return methodInfo.ReturnTypeCustomAttributes.GetReferenceNullability(methodInfo);
        }
    }
}
