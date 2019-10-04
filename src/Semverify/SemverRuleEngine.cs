using Semverify.ApiModel;
using Semverify.SemverRules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify
{
    internal class SemverRuleEngine
    {
        private static readonly List<ISemverRule> Rules = new List<ISemverRule>
        {
            new NewMemberRule(),
            new RemovedMemberRule(),
            new PropertyChangedRule(),
            new TypeChangedRule(),
            new MethodChangedRule(),
            new ConstructorChangedRule(),
            new EventChangedRule()
        };

        public IEnumerable<SemverRuleResult> InspectRules(ApiMemberInfo prior, ApiMemberInfo current)
        {
            foreach (var rule in Rules)
            {
                foreach (var ruleResult in rule.Inspect(prior, current))
                {
                    yield return ruleResult;
                }
            }
        }
    }
}
