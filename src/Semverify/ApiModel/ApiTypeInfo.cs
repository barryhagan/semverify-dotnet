using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Semverify.ApiModel
{
    internal class ApiTypeInfo : ApiMemberInfo
    {
        private static readonly Regex BackingMethodsRegex = new Regex("^get_|^set_|^add_|^remove");
        private static readonly string[] ExcludedBaseTypes = new string[] { typeof(object).FullName, typeof(Enum).FullName, typeof(ValueType).FullName, typeof(MulticastDelegate).FullName };

        protected readonly TypeInfo TypeInfo;

        public override string Namespace { get => TypeInfo.Namespace; }

        public ApiTypeInfo(MemberInfo memberInfo) : base(memberInfo)
        {
            if (!(memberInfo is TypeInfo typeInfo))
            {
                throw new ArgumentException("The memberInfo is not a type", nameof(memberInfo));
            }

            TypeInfo = typeInfo;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var indentSize = indentLevel * IndentSpaces;

            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";
            var baseType = GetBaseType();
            var impls = new List<string>();
            impls.AddIf(baseType != null, baseType);
            impls.AddRange(GetInterfaces().OrderBy(i => i));
            var implString = impls.Any() ? $" : {string.Join(", ", impls)}" : "";
            var constraints = ResolveConstraints(TypeInfo.GetGenericArguments());
            var constraintString = constraints.Any() ? $" {string.Join(" ", constraints)}" : "";
            var accessorString = $" {GetAccessor()}";

            var name = $"{modString}{GetLocalName()}{implString}{constraintString}";

            var apiBuilder = new StringBuilder();

            var members = EnumerateMembers().OrderBy(a => a, new MemberInfoDisplayComparer());

            if (members.Any())
            {
                apiBuilder.AppendLine($"{new string(' ', indentSize)}{name}");
                apiBuilder.AppendLine($"{new string(' ', indentSize)}{{");
                foreach (var member in members)
                {
                    apiBuilder.AppendLine(member.FormatForApiOutput(indentLevel + 1));
                }
                apiBuilder.AppendLine($"{new string(' ', indentSize)}}}");
            }
            else
            {
                apiBuilder.AppendLine($"{new string(' ', indentSize)}{name} {GetAccessor()}");
            }

            return apiBuilder.ToString();
        }

        public override string GetAccessor()
        {
            return "{ }";
        }

        public IList<string> GetConstraints()
        {
            return ResolveConstraints(TypeInfo.GetGenericArguments());
        }

        public override string GetFullName()
        {
            return TypeInfo.ResolveQualifiedName();
        }

        public IList<ApiTypeDetails> GetGenericArgs()
        {
            var nullableEnumerator = (TypeInfo.GetReferenceNullability() ?? new byte[] { 0 }).AsEnumerable().GetEnumerator();
            nullableEnumerator.MoveNext();

            return TypeInfo.GetGenericArguments().Select(g =>
            {
                var type = g.GetType();
                if (!type.IsValueType || type.IsGenericType)
                {
                    if (!nullableEnumerator.MoveNext())
                    {
                        // Assume if we don't have enough nullable bytes to represent the type
                        // that it used NullableAttribute(byte) instead of NullableAttribute(byte[])
                        // because all members have the same nullability and could be condensed
                        nullableEnumerator.Reset();
                        nullableEnumerator.MoveNext();
                    }
                }

                return new ApiTypeDetails(g, new byte[] { nullableEnumerator.Current });
            }).ToList();
        }

        public override string GetLocalName()
        {
            return TypeInfo.ResolveLocalName();
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
                mods.AddIf(TypeInfo.IsSealed, "sealed");
            }

            mods.AddIf(TypeInfo.IsClass, "class");

            return mods;
        }

        public override string GetSignature()
        {
            var mods = GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";

            var baseType = GetBaseType();

            var impls = new List<string>();
            impls.AddIf(baseType != null, baseType);
            impls.AddRange(GetInterfaces().OrderBy(i => i));
            var implString = impls.Any() ? $" : {string.Join(", ", impls)}" : "";

            var constraints = ResolveConstraints(TypeInfo.GetGenericArguments());
            var constraintString = constraints.Any() ? $" {string.Join(" ", constraints)}" : "";

            var accessorString = $" {GetAccessor()}";

            return $"{modString}{GetFullName()}{implString}{constraintString}{accessorString}";
        }

        public string GetBaseType()
        {
            if (TypeInfo.BaseType != null && !ExcludedBaseTypes.Contains(TypeInfo.BaseType.FullName))
            {
                return TypeInfo.BaseType.ResolveQualifiedName();
            }
            return null;
        }

        public virtual IList<string> GetInterfaces()
        {
            var implList = new List<string>();
            var interfaces = TypeInfo.GetInterfaces();
            var inheritedInterfaces = interfaces.SelectMany(i => i.GetInterfaces()).Concat(TypeInfo.BaseType?.GetInterfaces() ?? new Type[0]);
            foreach (var intface in TypeInfo.GetInterfaces().Except(inheritedInterfaces))
            {
                implList.Add(intface.ResolveQualifiedName());
            }
            return implList;
        }

        protected virtual IList<string> GetTypeAccessModifiers()
        {
            var mods = new List<string>();
            mods.AddFirstIf(new[]
            {
                (condition: TypeInfo.IsNestedFamANDAssem, value: "private protected"),
                (condition: TypeInfo.IsNestedFamORAssem, value: "protected internal"),
                (condition: TypeInfo.IsNestedAssembly, value: "internal"),
                (condition: TypeInfo.IsNestedFamily, value: "protected"),
                (condition: TypeInfo.IsPublic || TypeInfo.IsNestedPublic, value: "public"),
            });
            return mods;
        }

        public virtual IEnumerable<ApiMemberInfo> EnumerateMembers()
        {
            foreach (var member in TypeInfo.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (member.CustomAttributes.Any(d => d.AttributeType.AssemblyQualifiedName == typeof(CompilerGeneratedAttribute).AssemblyQualifiedName))
                {
                    continue;
                }

                if (member is MethodInfo method)
                {
                    if (method.IsPrivate || method.IsAssembly || method.IsFamilyAndAssembly)
                    {
                        continue;
                    }

                    if (method.IsHideBySig && method.IsSpecialName)
                    {
                        if (BackingMethodsRegex.IsMatch(method.Name))
                        {
                            continue;
                        }
                    }

                    yield return new ApiMethodInfo(method);
                }
                else if (member is FieldInfo field)
                {
                    if (field.IsPrivate || field.IsAssembly || field.IsFamilyAndAssembly)
                    {
                        continue;
                    }
                    yield return new ApiFieldInfo(field);
                }
                else if (member is ConstructorInfo ctor)
                {
                    if (ctor.IsPrivate || ctor.IsAssembly || ctor.IsFamilyAndAssembly)
                    {
                        continue;
                    }
                    yield return new ApiConstructorInfo(ctor);
                }
                else if (member is PropertyInfo property)
                {
                    var getter = property.GetMethod;
                    var getterAccessible = getter != null && !getter.IsPrivate && !getter.IsAssembly && !getter.IsFamilyAndAssembly;
                    var setter = property.SetMethod;
                    var setterAccessible = setter != null && !setter.IsPrivate && !setter.IsAssembly && !setter.IsFamilyAndAssembly;
                    if (!getterAccessible && !setterAccessible)
                    {
                        continue;
                    }
                    yield return new ApiPropertyInfo(property);
                }
                else if (member.MemberType == MemberTypes.Event)
                {
                    yield return new ApiEventInfo(member);
                }
                else if (member is TypeInfo nested)
                {
                    if (nested.IsNestedPrivate || nested.IsNestedAssembly || nested.IsNestedFamANDAssem)
                    {
                        continue;
                    }

                    if (!HasAbstractInternalMembers(nested))
                    {
                        yield return LoadType(nested);
                    }
                }
            }
        }

        public static bool HasAbstractInternalMembers(Type type)
        {
            if (type.IsAbstract && !type.IsInterface)
            {
                foreach (var member in type.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (member is MethodInfo method)
                    {
                        if (method.IsAbstract && (method.IsAssembly || method.IsFamilyAndAssembly))
                        {
                            return true;
                        }
                    }
                    else if (member is ConstructorInfo ctor)
                    {
                        if (ctor.IsAbstract && (ctor.IsAssembly || ctor.IsFamilyAndAssembly))
                        {
                            return true;
                        }
                    }
                    else if (member is PropertyInfo property)
                    {
                        var getter = property.GetMethod;
                        if ((getter?.IsAbstract ?? false) && (getter.IsAssembly || getter.IsFamilyAndAssembly))
                        {
                            return true;
                        }
                        var setter = property.SetMethod;
                        if ((setter?.IsAbstract ?? false) && (setter.IsAssembly || setter.IsFamilyAndAssembly))
                        {
                            return true;
                        }
                    }
                    else if (member.MemberType == MemberTypes.Event)
                    {
                        var addMethod = member.GetType().GetProperty("AddMethod", BindingFlags.Instance | BindingFlags.Public).GetValue(member, null) as MethodInfo;
                        if (addMethod.IsAbstract && (addMethod.IsAssembly || addMethod.IsFamilyAndAssembly))
                        {
                            return true;
                        }
                    }
                    else if (member is TypeInfo nested)
                    {
                        if (nested.IsAbstract && (nested.IsNestedAssembly || nested.IsNestedFamANDAssem))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static ApiTypeInfo LoadType(Type type)
        {
            if (type.BaseType?.FullName == typeof(MulticastDelegate).FullName)
            {
                return new ApiDelegateInfo(type, type.GetMember("Invoke").First() as MethodInfo);
            }
            else if (type.IsInterface)
            {
                return new ApiInterfaceInfo(type);
            }
            else if (type.IsEnum)
            {
                return new ApiEnumInfo(type);
            }
            else if (type.IsValueType)
            {
                return new ApiStructInfo(type);
            }
            else
            {
                return new ApiTypeInfo(type);
            }
        }
    }
}
