using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Semverify.ApiModel;
using Semverify.SemverModel;

namespace Semverify.SemverRules
{
    internal class ConstructorChangedRule : ISemverRule
    {
        public IEnumerable<SemverRuleResult> Inspect(ApiMemberInfo prior, ApiMemberInfo current)
        {
            if (prior == null || current == null)
            {
                yield break;
            }

            if (!(prior is ApiConstructorInfo priorConstructor) || !(current is ApiConstructorInfo currentConstructor))
            {
                yield break;
            }

            var modifiersEqual = priorConstructor.GetModifiers().SequenceEqual(currentConstructor.GetModifiers());
            if (!modifiersEqual)
            {
                yield return new SemverRuleResult("Modifiers changed", "The modifiers of this constructor changed", SemverChangeType.Major);
            }

            var priorParams = priorConstructor.GetParameters();
            var currentParams = currentConstructor.GetParameters();

            if (priorParams.Count() > currentParams.Count())
            {
                yield return new SemverRuleResult("Parameters removed", "Parameters were removed from this constructor.", SemverChangeType.Major);
            }
            else if (priorParams.Count() < currentParams.Count())
            {
                yield return new SemverRuleResult("Parameters added", "New parameters were added to this constructor.", SemverChangeType.Major);
            }
            else
            {
                var counter = 0;
                foreach (var (param1, param2) in priorParams.Zip(currentParams))
                {
                    counter++;
                    if (param1.Type.Name != param2.Type.Name)
                    {
                        yield return new SemverRuleResult($"Parameter {counter} type changed", $"The type of parameter {counter} was changed.", SemverChangeType.Major);
                    }
                    else if (param1.HasDefaultValue && !param2.HasDefaultValue)
                    {
                        yield return new SemverRuleResult($"Parameter {counter} is no longer optional", $"Parameter {counter} was changed to no longer have a default value.", SemverChangeType.Major);
                    }
                    else if (!param1.HasDefaultValue && param2.HasDefaultValue)
                    {
                        yield return new SemverRuleResult($"Parameter {counter} is now optional", $"Parameter {counter} was changed to be optional.", SemverChangeType.Minor);
                    }
                    else if (param1.DefaultValue != param2.DefaultValue)
                    {
                        yield return new SemverRuleResult($"Parameter {counter} default value changed", $"The default value of parameter {counter} was changed.", SemverChangeType.Minor);
                    }
                    else if (param1.ReferenceNullabilityHash != param2.ReferenceNullabilityHash)
                    {
                        yield return new SemverRuleResult($"Parameter {counter} nullability changed", $"The reference nullability of parameter {counter} was changed.", SemverChangeType.Patch);
                    }
                    else if (param1.Name != param2.Name)
                    {
                        yield return new SemverRuleResult($"Parameter {counter} name changed", $"The name of parameter {counter} was changed.", SemverChangeType.Minor);
                    }
                }
            }
        }
    }
}
