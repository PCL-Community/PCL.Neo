using PCL.Neo.Core.Models.Minecraft.Game.Data.GaJvArguments;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public static class ArgumentProcessor
{
    private static bool CheckOsRule(OsSpec? ruleOs, CurrentEnvironment environment)
    {
        if (ruleOs == null) return true; // No OS rule, so this part matches.

        if (!string.IsNullOrEmpty(ruleOs.Name) &&
            !ruleOs.Name.Equals(environment.OsName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(ruleOs.Arch) &&
            !ruleOs.Arch.Equals(environment.OsArch, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(ruleOs.Version))
        {
            try
            {
                // Regex matching for version
                if (!Regex.IsMatch(environment.OsVersion, ruleOs.Version)) return false;
            }
            catch (ArgumentException ex) // Invalid regex pattern in rule
            {
                Console.Error.WriteLine($"Warning: Invalid OS version regex '{ruleOs.Version}': {ex.Message}");
                return false; // Treat invalid regex as non-match
            }
        }

        return true;
    }

    private static bool CheckFeaturesRule(KeyValuePair<string, JsonElement> ruleFeature, CurrentEnvironment environment)
    {
        return environment.ActiveFeatures.FeatureCunstomValue.TryGetValue(ruleFeature.Key, out bool value) && value;
    }

    private static bool AreRulesSatisfied(List<GaJvArguments.Rule> rulesList, CurrentEnvironment environment)
    {
        if (rulesList.Count == 0)
        {
            return true; // No rules, default allow.
        }

        bool overallPermission = true; // Default to allow, a rule can change this to false.

        foreach (var rule in rulesList)
        {
            if (string.IsNullOrEmpty(rule.Action)) continue; // Skip malformed rules

            bool osConditionsMet = CheckOsRule(rule.Os, environment);
            bool featureConditionsMet = rule.Features == null || CheckFeaturesRule(rule.Features.Feature, environment);

            // For a rule to apply its conditions, ALL its specified conditions (OS and Feature) must be met.
            bool ruleCriteriaMet = osConditionsMet && featureConditionsMet;

            if (rule.Action.Equals("allow", StringComparison.OrdinalIgnoreCase))
            {
                // If an "allow" rule has conditions (OS or Feature specified) AND those conditions are NOT met,
                // then this argument is disallowed by this rule.
                bool ruleHasAnyConditions = (rule.Os != null || rule.Features != null);
                if (ruleHasAnyConditions && !ruleCriteriaMet)
                {
                    overallPermission = false;
                    break; // This specific "allow" rule (for other conditions) blocks the argument.
                }
                // If "allow" rule has no conditions, or its conditions ARE met, it doesn't change overallPermission if it's already true.
            }
            else if (rule.Action.Equals("disallow", StringComparison.OrdinalIgnoreCase))
            {
                // If a "disallow" rule's criteria ARE met, then this argument is disallowed.
                if (ruleCriteriaMet)
                {
                    overallPermission = false;
                    break; // This "disallow" rule (matching current conditions) blocks the argument.
                }
                // If a "disallow" rule's criteria are NOT met, it's a "disallow" for other conditions, so it's ignored for current.
            }
            // Else: unknown action, ignore the rule or log a warning.
        }

        return overallPermission;
    }

    public static List<string> GetEffectiveArguments(
        List<ArgumentElement> argumentElements,
        CurrentEnvironment environment)
    {
        List<string> finalArgs = [];

        foreach (var element in argumentElements)
        {
            switch (element)
            {
                case StringArgument strArg:
                    finalArgs.Add(strArg.Value);
                    break;
                case RuledArgument ruledArg:
                    {
                        if (AreRulesSatisfied(ruledArg.Rules, environment))
                        {
                            finalArgs.AddRange(ruledArg.Value);
                        }

                        break;
                    }
            }
        }

        return finalArgs;
    }
}