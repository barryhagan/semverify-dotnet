using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Semverify.Tests")]
namespace Semverify.ApiModel
{
    internal abstract class ApiMemberInfo
    {
        internal const int IndentSpaces = 4;

        public readonly MemberInfo MemberInfo;

        public MemberTypes MemberType { get => MemberInfo.MemberType; }

        public virtual string Namespace { get => MemberInfo.DeclaringType.Namespace; }

        public ApiMemberInfo(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }

        public abstract string FormatForApiOutput(int indentLevel = 0);
            
        public abstract string GetAccessor();

        public abstract string GetFullName();

        public abstract string GetLocalName();

        public abstract IList<string> GetModifiers();

        public abstract string GetSignature();

        protected virtual string FormatGenericArgs(MethodInfo method, bool applyGenericModifiers = true)
        {
            var genericArgs = method.GetGenericArguments()
                .OrderBy(a => a.GenericParameterPosition)
                .Select(a => $"{a.ResolveQualifiedName(applyGenericModifiers: applyGenericModifiers)}");

            return genericArgs.Any() ? $"<{string.Join(", ", genericArgs)}>" : "";
        }

        protected virtual List<string> ResolveConstraints(Type[] genericArgs)
        {
            var templateConstraints = new List<string>();
            foreach (var genericArg in genericArgs)
            {
                var constraintList = new List<string>();

                constraintList.AddRange(genericArg.GetGenericParameterConstraints().Select(t => t.ResolveQualifiedName()));

                var constraints = genericArg.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
                if ((constraints & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                    constraintList.Add("class");
                if ((constraints & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                    constraintList.Add("struct");
                if ((constraints & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                    constraintList.Add("new()");

                if (constraintList.Any())
                {
                    templateConstraints.Add($"where {genericArg.ResolveQualifiedName()} : {string.Join(", ", constraintList)}");
                }
            }
            return templateConstraints;
        }

        protected virtual IList<string> ResolveParameters(ParameterInfo[] parameters, bool applyGenericModifiers = true, MemberInfo context = null)
        {
            var paramList = new List<string>();
            foreach (var param in parameters)
            {
                var mods = new List<string>();
                mods.AddIf(param.ParameterType.IsByRef && !param.IsOut, "ref");
                if (applyGenericModifiers)
                {
                    mods.AddIf(param.IsIn, "in");
                    mods.AddIf(param.ParameterType.IsByRef && param.IsOut, "out");
                }
                mods.AddIf(param.HasAttribute(typeof(ParamArrayAttribute)), "params");
                var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

                string defaultValue = string.Empty;
                if (param.IsOptional)
                {
                    try
                    {
                        var defaultVal = param.RawDefaultValue?.ToString();
                        if (param.ParameterType.FullName == "System.String" && defaultVal != null)
                        {
                            defaultVal = $"\"{defaultVal}\"";
                        }
                        defaultValue = $" = {defaultVal ?? "null"}";
                    }
                    catch (BadImageFormatException)
                    {
                        defaultValue = " = ???";
                    }
                }

                paramList.Add($"{modString}{param.ParameterType.ResolveQualifiedName(param.GetReferenceNullability(context ?? MemberInfo), applyGenericModifiers)} {param.Name}{defaultValue}");
            }
            return paramList;
        }

        protected virtual bool HasNewModifier(Type baseType, MemberInfo member)
        {
            while (baseType != null)
            {
                var baseMember = baseType.GetMembers().FirstOrDefault(m => m.Name == member.Name);
                if (baseMember != null)
                {
                    if (baseMember.MemberType == MemberTypes.Method && member.MemberType == MemberTypes.Method)
                    {
                        var baseMethod = baseMember as MethodInfo;
                        var method = member as MethodInfo;

                        if (baseMethod.GetParameters().Select(p => p.ParameterType.FullName).SequenceEqual(method.GetParameters().Select(p => p.ParameterType.FullName)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
