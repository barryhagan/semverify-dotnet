using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Semverify
{
    internal static class ReflectionExtensions
    {
        private static readonly Regex GenericParamsRegex = new Regex(@"`\d+");
        private static readonly Dictionary<string, string> TypeAliases = new Dictionary<string, string>
        {
            { typeof(object).FullName, "object" },
            { typeof(void).FullName, "void" },
            { typeof(string).FullName, "string" },
            { typeof(bool).FullName, "bool" },
            { typeof(char).FullName, "char" },
            { typeof(float).FullName, "float" },
            { typeof(double).FullName, "double" },
            { typeof(decimal).FullName, "decimal" },
            { typeof(sbyte).FullName, "sbyte" },
            { typeof(byte).FullName, "byte" },
            { typeof(short).FullName, "short" },
            { typeof(ushort).FullName, "ushort" },
            { typeof(int).FullName, "int" },
            { typeof(uint).FullName, "uint" },
            { typeof(long).FullName, "long" },
            { typeof(ulong).FullName, "ulong" },
            { typeof(Enum).FullName, "enum" },
        };

        public static bool HasAttribute(this MemberInfo info, Type attributeType)
        {
            return info.CustomAttributes.Any(cad => cad.AttributeType.FullName == attributeType.FullName);
        }

        public static bool HasAttribute(this ParameterInfo info, Type attributeType)
        {
            return info.CustomAttributes.Any(cad => cad.AttributeType.FullName == attributeType.FullName);
        }

        public static string ResolveLocalName(this Type type, bool applyGenericModifiers = true)
        {
            return ResolveDisplayName(type, false, applyGenericModifiers);
        }

        public static string ResolveQualifiedName(this Type type, bool applyGenericModifiers = true)
        {
            return ResolveDisplayName(type, true, applyGenericModifiers);
        }

        private static string ResolveDisplayName(Type type, bool withNamespace = true, bool applyGenericModifiers = true)
        {
            if (type == null)
            {
                return null;
            }

            var suffix = string.Empty;
            var resolvedType = type;
            var innerType = resolvedType.GetElementType();
            while (innerType != null)
            {
                if (!resolvedType.IsByRef)
                {
                    suffix = resolvedType.Name.Substring(innerType.Name.Length) + suffix;
                }
                resolvedType = innerType;
                innerType = innerType.GetElementType();
            }

            var typeName = $"{resolvedType.Name}{suffix}";
            if (TypeAliases.TryGetValue(resolvedType.FullName ?? string.Empty, out var alias))
            {
                typeName = $"{alias}{suffix}";
            }

            if (resolvedType.IsGenericParameter)
            {
                if (applyGenericModifiers)
                {
                    switch (resolvedType.GenericParameterAttributes)
                    {
                        case GenericParameterAttributes.Contravariant:
                            return $"in {typeName}";
                        case GenericParameterAttributes.Covariant:
                            return $"out {typeName}";
                    }
                }

                return $"{typeName}";
            }

            var prefix = withNamespace && string.IsNullOrWhiteSpace(alias) ? $"{type.Namespace}." : "";
            if (type.IsNested && withNamespace)
            {
                prefix = $"{ResolveDisplayName(type.DeclaringType, withNamespace, applyGenericModifiers)}.";
            }

            if (type.GetGenericArguments().Any())
            {
                var genericArgs = type.GetGenericArguments().Select(a => ResolveDisplayName(a, applyGenericModifiers: applyGenericModifiers));
                return $"{prefix}{GenericParamsRegex.Replace($"{typeName}", $"<{string.Join(", ", genericArgs)}>")}";
            }

            return $"{prefix}{typeName}";
        }
    }
}
