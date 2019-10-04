using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Semverify.ApiModel;
using Semverify.SemverModel;

namespace Semverify.SemverRules
{
    internal class EventChangedRule : ISemverRule
    {
        public IEnumerable<SemverRuleResult> Inspect(ApiMemberInfo prior, ApiMemberInfo current)
        {
            if (prior == null || current == null)
            {
                yield break;
            }

            if (!(prior is ApiEventInfo priorEvent) || !(current is ApiEventInfo currentEvent))
            {
                yield break;
            }

            if (!priorEvent.GetModifiers().SequenceEqual(currentEvent.GetModifiers()))
            {
                yield return new SemverRuleResult("Modifiers changed", "The modifiers of this event were changed", SemverChangeType.Major);
            }

            var priorTypeDetail = priorEvent.GetEventType();
            var currentTypeDetail = currentEvent.GetEventType();

            if (priorTypeDetail.Type.Name != currentTypeDetail.Type.Name)
            {
                yield return new SemverRuleResult("Type changed", "The type of this event was changed", SemverChangeType.Major);
            }
            else if (priorTypeDetail.ReferenceNullabilityHash != currentTypeDetail.ReferenceNullabilityHash)
            {
                yield return new SemverRuleResult($"Nullability changed", "The reference nullability of this event was changed", SemverChangeType.Patch);
            }
        }
    }
}
