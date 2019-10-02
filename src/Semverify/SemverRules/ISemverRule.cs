using Semverify.ApiModel;
using Semverify.SemverModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.SemverRules
{
    internal interface ISemverRule
    {
        IEnumerable<SemverRuleResult> Inspect(ApiMemberInfo prior, ApiMemberInfo current);
    }
}
