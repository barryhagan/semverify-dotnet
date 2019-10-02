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
        }
    }
}
