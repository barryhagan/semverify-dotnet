﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Semverify.ApiModel
{
    internal class ApiPropertyInfo : ApiMemberInfo
    {
        private readonly PropertyInfo propertyInfo;

        public ApiPropertyInfo(MemberInfo memberInfo) : base(memberInfo)
        {
            if (!(memberInfo is PropertyInfo property))
            {
                throw new ArgumentException("The memberInfo is not a property", nameof(memberInfo));
            }

            propertyInfo = property;
        }

        public override string FormatForApiOutput(int indentLevel = 0)
        {
            var mods = MemberInfo.DeclaringType.IsInterface ? new List<string>() : GetModifiers();
            var modString = mods.Any() ? $"{string.Join(" ", mods)} " : "";
            var accessor = GetAccessor();
            return $"{new string(' ', indentLevel * IndentSpaces)}{modString}{propertyInfo.PropertyType.ResolveQualifiedName()} {GetLocalName()} {accessor}";
        }

        public override string GetFullName()
        {
            return $"{propertyInfo.DeclaringType.ResolveQualifiedName()}.{propertyInfo.Name}";
        }

        public override string GetLocalName()
        {
            if (propertyInfo.IsSpecialName)
            {
                if (propertyInfo.Name == "Item")
                {
                    var indexer = propertyInfo.GetIndexParameters().FirstOrDefault();
                    if (indexer != null)
                    {
                        return $"this[{indexer.ParameterType.ResolveQualifiedName()} {indexer.Name}]";
                    }
                }
            }

            return propertyInfo.Name;
        }

        public override IList<string> GetModifiers()
        {
            var mods = new List<string>();

            if (MemberInfo.DeclaringType.IsInterface)
            {
                mods.Add("public");
                return mods;
            }

            var access = (propertyInfo.GetGetMethod(true)?.Attributes ?? 0) | (propertyInfo.GetSetMethod(true)?.Attributes ?? 0);

            mods.AddFirstIf(new[] {
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
            var accessor = GetAccessor();
            return $"{modString}{propertyInfo.PropertyType.ResolveQualifiedName()} {GetFullName()} {accessor}";
        }

        public override string GetAccessor()
        {
            var mods = GetModifiers();
            var getter = propertyInfo.GetGetMethod(true);
            var setter = propertyInfo.GetSetMethod(true);

            var accessString = new StringBuilder("{");
            if (getter != null && !getter.IsPrivate)
            {
                var getterMods = new ApiMethodInfo(getter).GetModifiers().Except(mods);
                var getterString = getterMods.Any() ? $" {string.Join(" ", getterMods)} get;" : " get;";
                accessString.Append(getterString);
            }
            if (setter != null && !setter.IsPrivate)
            {
                var setterMods = new ApiMethodInfo(setter).GetModifiers().Except(mods);
                var setterString = setterMods.Any() ? $" {string.Join(" ", setterMods)} set;" : " set;";
                accessString.Append(setterString);
            }
            accessString.Append(" }");
            return accessString.ToString();
        }
    }
}