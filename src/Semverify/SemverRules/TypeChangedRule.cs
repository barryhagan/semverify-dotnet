using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Semverify.ApiModel;
using Semverify.SemverModel;

namespace Semverify.SemverRules
{
    internal class TypeChangedRule : ISemverRule
    {
        public IEnumerable<SemverRuleResult> Inspect(ApiMemberInfo prior, ApiMemberInfo current)
        {
            if (prior == null || current == null)
            {
                yield break;
            }

            if (!(prior is ApiTypeInfo priorType) || !(current is ApiTypeInfo currentType))
            {
                yield break;
            }

            var modifiersEqual = priorType.GetModifiers().SequenceEqual(currentType.GetModifiers());
            if (!modifiersEqual)
            {
                yield return new SemverRuleResult("Modifiers changed", "The modifiers of this type were changed", SemverChangeType.Major);
            }

            if (!priorType.GetInterfaces().SequenceEqual(currentType.GetInterfaces()))
            {
                if (modifiersEqual && currentType.GetModifiers().Contains("sealed"))
                {
                    yield return new SemverRuleResult("Interfaces changed on a sealed class", "The implemented interfaces of this sealed type were changed", SemverChangeType.Patch);
                }
                else
                {
                    yield return new SemverRuleResult("Interfaces changed", "The implemented interfaces of this type were changed", SemverChangeType.Major);
                }
            }

            if (!priorType.GetBaseType()?.Equals(currentType.GetBaseType()) ?? false)
            {
                yield return new SemverRuleResult("Base type changed", "The base type for this derived type was changed.", SemverChangeType.Major);
            }

            var priorArgs = priorType.GetGenericArgs();
            var currentArgs = currentType.GetGenericArgs();

            if (priorArgs.Count() > currentArgs.Count())
            {
                yield return new SemverRuleResult("Generic arguments removed", "Generic arguments were removed from this type.", SemverChangeType.Major);
            }
            else if (priorArgs.Count() < currentArgs.Count())
            {
                yield return new SemverRuleResult("Parameters added", "Generic arguments were added to this type.", SemverChangeType.Major);
            }
            else
            {
                var counter = 0;
                foreach (var (param1, param2) in priorArgs.Zip(currentArgs))
                {
                    counter++;
                    if (param1.Type.Name != param2.Type.Name)
                    {
                        yield return new SemverRuleResult($"Generic argument {counter} type changed", $"The type of argument {counter} was changed.", SemverChangeType.Major);
                    }
                    else if (param1.ReferenceNullabilityHash != param2.ReferenceNullabilityHash)
                    {
                        yield return new SemverRuleResult($"Generic argument {counter} nullability changed", $"The reference nullability of argument {counter} was changed.", SemverChangeType.Patch);
                    }
                }
            }

            var priorConstraints = priorType.GetConstraints();
            var currentConstraints = currentType.GetConstraints();

            if (!priorConstraints.SequenceEqual(currentConstraints))
            {
                yield return new SemverRuleResult("Constraints changed", "The generic argument constraints for this type were changed", SemverChangeType.Major);
            }
        }
    }
}
