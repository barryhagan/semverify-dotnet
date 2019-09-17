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

            if (type.IsGenericParameter)
            {
                if (applyGenericModifiers)
                {
                    switch (type.GenericParameterAttributes)
                    {
                        case GenericParameterAttributes.Contravariant:
                            return $"in {type.Name}";
                        case GenericParameterAttributes.Covariant:
                            return $"out {type.Name}";
                    }
                }

                return type.Name;
            }

            var prefix = withNamespace ? $"{type.Namespace}." : "";
            if (type.IsNested && withNamespace)
            {
                prefix = $"{ResolveDisplayName(type.DeclaringType, withNamespace, applyGenericModifiers)}.";
            }

            if (type.GetGenericArguments().Any())
            {
                var genericArgs = type.GetGenericArguments().Select(a => ResolveDisplayName(a, applyGenericModifiers: applyGenericModifiers));
                return $"{prefix}{GenericParamsRegex.Replace(type.Name, $"<{string.Join(", ", genericArgs)}>")}";
            }

            return ResolveTypeAlias($"{prefix}{type.Name}");
        }

        private static string ResolveTypeAlias(string typeName)
        {
            var matchName = typeName ?? string.Empty;
            var isArray = matchName.EndsWith("[]");
            if (isArray)
            {
                matchName = typeName.Substring(0, typeName.Length - 2);
            }
            if (TypeAliases.TryGetValue(matchName, out var alias))
            {
                return isArray ? $"{alias}[]" : alias;
            }

            return typeName;
        }
    }
}
