using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Semverify.ApiModel;
using Semverify.SemverModel;

namespace Semverify.SemverRules
{
    internal class MethodChangedRule : ISemverRule
    {
        public IEnumerable<SemverRuleResult> Inspect(ApiMemberInfo prior, ApiMemberInfo current)
        {
            if (prior == null || current == null)
            {
                yield break;
            }

            if (!(prior is ApiMethodInfo priorMethod) || !(current is ApiMethodInfo currentMethod))
            {
                yield break;
            }

            var modifiersEqual = priorMethod.GetModifiers().SequenceEqual(currentMethod.GetModifiers());
            if (!modifiersEqual)
            {
                yield return new SemverRuleResult("Modifiers changed", "The modifiers of this method were changed", SemverChangeType.Major);
            }

            var priorReturn = priorMethod.GetReturnParameter();
            var currentReturn = currentMethod.GetReturnParameter();

            if (priorReturn.Type.Name != currentReturn.Type.Name)
            {
                yield return new SemverRuleResult("Return type changed", "The return type of this method was changed", SemverChangeType.Major);
            }
            else if (priorReturn.ReferenceNullabilityHash != currentReturn.ReferenceNullabilityHash)
            {
                yield return new SemverRuleResult($"Return type nullability changed", "The reference nullability of this method's return type was changed", SemverChangeType.Patch);
            }

            var priorParams = priorMethod.GetParameters();
            var currentParams = currentMethod.GetParameters();

            if (priorParams.Count() > currentParams.Count())
            {
                yield return new SemverRuleResult("Parameters removed", "Parameters were removed from this method.", SemverChangeType.Major);
            }
            else if (priorParams.Count() < currentParams.Count())
            {
                yield return new SemverRuleResult("Parameters added", "New parameters were added to this method.", SemverChangeType.Major);
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
                    else if (param1.HasDefaultValue &&  !param2.HasDefaultValue)
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
