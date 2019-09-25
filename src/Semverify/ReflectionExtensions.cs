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

        public static string ResolveLocalName(this Type type, byte[] nullableAttribute = null, bool applyGenericModifiers = true)
        {
            return ResolveDisplayName(type, nullableAttribute?.AsEnumerable().GetEnumerator(), false, applyGenericModifiers);
        }

        public static string ResolveQualifiedName(this Type type, byte[] nullableAttribute = null, bool applyGenericModifiers = true)
        {
            return ResolveDisplayName(type, nullableAttribute?.AsEnumerable().GetEnumerator(), applyGenericModifiers);
        }

        private static string ResolveDisplayName(Type type, IEnumerator<byte> nullableEnumerator, bool withNamespace = true, bool applyGenericModifiers = true)
        {
            if (type == null)
            {
                return null;
            }

            if (nullableEnumerator == null)
            {
                nullableEnumerator = new byte[] { 0 }.AsEnumerable().GetEnumerator();
            }

            var suffix = string.Empty;
            var resolvedType = type;
            var innerType = resolvedType.GetElementType();
            while (innerType != null)
            {
                if (!resolvedType.IsByRef)
                {
                    suffix = $"{resolvedType.Name.Substring(innerType.Name.Length)}{suffix}";
                }

                //check if element is nullable reference type
                if (!resolvedType.IsValueType || resolvedType.IsGenericType)
                {
                    if (nullableEnumerator.MoveNext() && nullableEnumerator.Current == 2)
                    {
                        suffix = $"{suffix}?";
                    }
                }

                resolvedType = innerType;
                innerType = innerType.GetElementType();
            }

            //nullable value types
            var underlyingNullable = Nullable.GetUnderlyingType(resolvedType);
            if (underlyingNullable != null)
            {
                resolvedType = underlyingNullable;
                suffix = $"?{suffix}";
            }

            //nullable reference types
            if (!resolvedType.IsValueType || resolvedType.IsGenericType)
            {
                if (nullableEnumerator.MoveNext() && nullableEnumerator.Current == 2)
                {
                    suffix = $"?{suffix}";
                }
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
                prefix = $"{ResolveDisplayName(type.DeclaringType, null, withNamespace, applyGenericModifiers)}.";
            }

            if (type.GetGenericArguments().Any())
            {
                var genericArgs = type.GetGenericArguments().Select(a => ResolveDisplayName(a, nullableEnumerator, applyGenericModifiers: applyGenericModifiers));
                return $"{prefix}{GenericParamsRegex.Replace($"{typeName}", $"<{string.Join(", ", genericArgs)}>")}";
            }

            return $"{prefix}{typeName}";
        }

        public static byte[] GetReferenceNullability(this ICustomAttributeProvider provider, MemberInfo context)
        {
            // bleh.  Currently not allowed to call GetCustomAttributes from ICustomAttributeProvider when using MetadataLoadContext.
            //var nullableAttribute = provider.GetCustomAttributes(false).FirstOrDefault(a => (a as CustomAttributeData).AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            //var nullableContextAttribute = provider.GetCustomAttributes(true).FirstOrDefault(a => (a as CustomAttributeData).AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            var customAttributeObject = provider.GetType().GetProperty("CustomAttributes").GetValue(provider);
            var customAttributes = (customAttributeObject as IEnumerable<CustomAttributeData>) ?? new CustomAttributeData[0];

            return GetReferenceNullability(customAttributes, context);
        }

        public static byte[] GetReferenceNullability(this MemberInfo member)
        {
            return GetReferenceNullability(member.CustomAttributes, member);
        }

        private static byte[] GetReferenceNullability(IEnumerable<CustomAttributeData> attributes, MemberInfo context)
        {
            var nullableAttribute = attributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

            if (nullableAttribute != null)
            {
                return nullableAttribute.ConstructorArguments[0].Value switch
                {
                    byte b =>new byte[] { b },
                    IEnumerable<CustomAttributeTypedArgument> arguments => arguments.Select(a=> (byte)a.Value).ToArray(),
                    _ => null
                };
            }

            var nullableContextAttribute = attributes.FirstOrDefault(a => (a as CustomAttributeData).AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (nullableContextAttribute != null)
            {
                return new byte[] { (byte)(nullableContextAttribute as CustomAttributeData).ConstructorArguments[0].Value };
            }

            while (context != null)
            {
                var scopeContextAttribute = context.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
                if (scopeContextAttribute != null)
                {
                    return new byte[] { (byte)scopeContextAttribute.ConstructorArguments[0].Value };
                }

                context = context.DeclaringType;
            }

            return null;
        }
    }
}
