using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Semverify.ApiModel;
using Semverify.SemverModel;

namespace Semverify.SemverRules
{
    internal class PropertyChangedRule : ISemverRule
    {
        public IEnumerable<SemverRuleResult> Inspect(ApiMemberInfo prior, ApiMemberInfo current)
        {
            if(prior == null || current == null)
            {
                yield break;
            }

            if (!(prior is ApiPropertyInfo priorProperty) || !(current is ApiPropertyInfo currentProperty))
            {
                yield break;
            }

            if (priorProperty.HasGetter && !currentProperty.HasGetter)
            {
                yield return new SemverRuleResult("Getter removed", "The get accessor was removed from this property", SemverChangeType.Major);
            }
            else if (currentProperty.HasGetter && !priorProperty.HasGetter)
            {
                yield return new SemverRuleResult("Getter added", "A get accessor was added to this property", SemverChangeType.Minor);
            }

            if (priorProperty.HasSetter && !currentProperty.HasSetter)
            {
                yield return new SemverRuleResult("Setter removed", "The set accessor was removed from this property", SemverChangeType.Major);
            }
            else if (currentProperty.HasSetter && !priorProperty.HasSetter)
            {
                yield return new SemverRuleResult("Setter added", "A set accessor was added to this property", SemverChangeType.Minor);
            }

            if (!priorProperty.GetModifiers().SequenceEqual(currentProperty.GetModifiers()))
            {
                yield return new SemverRuleResult("Modifiers changed", "The modifiers of this property were changed", SemverChangeType.Major);
            }

            var priorTypeDetail = priorProperty.GetPropertyType();
            var currentTypeDetail = currentProperty.GetPropertyType();

            if (priorTypeDetail.Type.Name != currentTypeDetail.Type.Name)
            {
                yield return new SemverRuleResult("Type changed", "The type of this property was changed", SemverChangeType.Major);
            }
            else if (priorTypeDetail.ReferenceNullabilityHash != currentTypeDetail.ReferenceNullabilityHash)
            {
                yield return new SemverRuleResult($"Nullability changed", "The reference nullability of this property was changed", SemverChangeType.Patch);
            }
        }
    }
}
