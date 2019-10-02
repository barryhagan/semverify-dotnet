using Semverify.SemverModel;

namespace Semverify.SemverRules
{
    internal struct SemverRuleResult
    {
        public string RuleName { get; }
        public string RuleDescription { get; }
        public SemverChangeType ChangeType { get; }

        public SemverRuleResult(string ruleName, string ruleDescription, SemverChangeType changeType)
        {
            RuleName = ruleName;
            RuleDescription = ruleDescription;
            ChangeType = changeType;
        }
    }
}
