using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Semverify.ApiModel
{
    internal class ApiTypeDetails
    {
        private readonly bool applyGenericModifiers = true;

        public byte[] ReferenceNullability { get; private set; }
        public Type Type { get; private set; }
        public string Name { get; private set; }
        public List<string> Modifiers { get; private set; } = new List<string>();
        public string DefaultValue { get; private set; }

        public bool HasDefaultValue { get => !string.IsNullOrWhiteSpace(DefaultValue); }

        public double ReferenceNullabilityHash { get => (ReferenceNullability ?? Array.Empty<byte>()).Select((b, i) => b == 2 ? 1 << i : 0).Sum(); }

        public ApiTypeDetails(Type parameterType, byte[] refNullability) : this(null, parameterType, refNullability)
        {
        }

        public ApiTypeDetails(ParameterInfo param, byte[] refNullability, bool applyGenericModifiers = true) : this(param.Name, param.ParameterType, refNullability)
        {
            this.applyGenericModifiers = applyGenericModifiers;

            Modifiers.AddIf(param.ParameterType.IsByRef && !param.IsOut, "ref");
            if (applyGenericModifiers)
            {
                Modifiers.AddIf(param.IsIn, "in");
                Modifiers.AddIf(param.ParameterType.IsByRef && param.IsOut, "out");
            }
            Modifiers.AddIf(param.HasAttribute(typeof(ParamArrayAttribute)), "params");

            if (param.HasDefaultValue)
            {
                try
                {
                    var defaultVal = param.RawDefaultValue?.ToString();
                    if (param.ParameterType.FullName == "System.String" && defaultVal != null)
                    {
                        defaultVal = $"\"{defaultVal}\"";
                    }
                    else if (param.ParameterType.IsValueType && !param.ParameterType.IsGenericType && defaultVal == null)
                    {
                        defaultVal = "default";
                    }

                    DefaultValue = $" = {defaultVal ?? "null"}";
                }
                catch (BadImageFormatException)
                {
                    DefaultValue = " = ???";
                }
            }
        }

        public ApiTypeDetails(string name, Type parameterType, byte[] refNullability)
        {
            Name = name;
            Type = parameterType;
            ReferenceNullability = Type.IsValueType ? new byte[] { 0 } : refNullability;
        }

        public override string ToString()
        {
            var modString = Modifiers.Any() ? $"{string.Join(" ", Modifiers)} " : "";
            var nameString = string.IsNullOrWhiteSpace(Name) ? "" : $" {Name}";
            return $"{modString}{Type.ResolveQualifiedName(ReferenceNullability, applyGenericModifiers)}{nameString}{DefaultValue}";
        }
    }
}
