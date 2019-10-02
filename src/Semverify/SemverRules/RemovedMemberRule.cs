using System;
using System.Collections.Generic;
using System.Text;
using Semverify.ApiModel;
using Semverify.SemverModel;

namespace Semverify.SemverRules
{
    internal class RemovedMemberRule : ISemverRule
    {
        private static readonly SemverRuleResult removedResult = new SemverRuleResult(
            "Removed member",
            "A member that is no longer present in the current version of the API",
            SemverChangeType.Major);

        public IEnumerable<SemverRuleResult> Inspect(ApiMemberInfo prior, ApiMemberInfo current)
        {
            if (current == null && prior != null)
            {
                yield return removedResult;
            }
        }
    }
}
