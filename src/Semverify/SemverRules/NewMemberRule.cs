using System;
using System.Collections.Generic;
using System.Text;
using Semverify.ApiModel;
using Semverify.SemverModel;

namespace Semverify.SemverRules
{
    internal class NewMemberRule : ISemverRule
    {
        private static readonly SemverRuleResult newMemberResult = new SemverRuleResult(
            "New member",
            "A new member that is not present in the prior version of the API",
            SemverChangeType.Minor);

        public IEnumerable<SemverRuleResult> Inspect(ApiMemberInfo prior, ApiMemberInfo current)
        {
            if (prior == null && current != null)
            {
                yield return newMemberResult;
            }
        }
    }
}
