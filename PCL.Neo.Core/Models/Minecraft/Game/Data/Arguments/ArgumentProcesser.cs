using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;
using PCL.Neo.Core.Utils;
using PCL.Neo.Core.Utils.Logger;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments;

public static partial class ArgumentProcessor
{
    private static readonly CurrentEnvironment Environment = CurrentEnvironment.GetCurrent();

    private static bool CheckOsRule(OsSpec? ruleOs)
    {
        if (ruleOs == null) return true; // No OS rule, so this part matches.

        if (!string.IsNullOrEmpty(ruleOs.Name) && // check os name
            !ruleOs.Name.Equals(Environment.OsName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(ruleOs.Arch) && // check arch
            !ruleOs.Arch.Equals(Environment.OsArch, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(ruleOs.Version)) // check os version
        {
            try
            {
                // Regex matching for version
                if (!Regex.IsMatch(Environment.OsVersion, ruleOs.Version)) return false;
            }
            catch (ArgumentException ex) // Invalid regex pattern in rule
            {
                NewLogger.Logger.LogError($"Warning: Invalid OS version regex '{ruleOs.Version}'.", ex);
                return false; // Treat invalid regex as non-match
            }
        }

        return true;
    }

    private static bool CheckFeaturesRule(Dictionary<string, JsonElement>? ruleFeature, ArgumentsAdapter adapter)
    {
        if (ruleFeature is null)
        {
            return true;
        }

        return adapter.Features.FeatureCustomValue.TryGetValue(ruleFeature.First().Key, out bool value) && value;
        // only use the first feature
    }

    private static bool AreRulesSatisfied(List<Rule> rulesList, ArgumentsAdapter adapter)
    {
        if (rulesList.Count == 0)
        {
            return true; // No rules, default allow.
        }

        bool overallPermission = true; // Default to allow, a rule can change this to false.

        foreach (var rule in rulesList)
        {
            if (string.IsNullOrEmpty(rule.Action)) continue; // Skip malformed rules

            var osConditionsMet = CheckOsRule(rule.Os);
            var featureConditionsMet = rule.Features == null || CheckFeaturesRule(rule.Features.Feature, adapter);

            // For a rule to apply its conditions, ALL its specified conditions (OS and Feature) must be met.
            var ruleCriteriaMet = osConditionsMet && featureConditionsMet;

            if (rule.Action.Equals("allow", StringComparison.OrdinalIgnoreCase))
            {
                // If an "allow" rule has conditions (OS or Feature specified) AND those conditions are NOT met,
                // then this argument is disallowed by this rule.
                bool ruleHasAnyConditions = rule.Os != null || rule.Features != null;
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

    private static bool ReplaceArgument(string argument, ArgumentsAdapter adapter, out string result)
    {
        if (argument[0] == '$')
        {
            result = adapter.Arguments.TryGetValue(argument, out var value)
                ? value
                : throw new ArgumentException($"Argument not found in Arguments: {argument}");
            return true;
        }
        else
        {
            result = argument;
            return false;
        }
    }

    /// <summary>
    /// Process customed properties.
    /// </summary>
    /// <param name="argument">The arguments the need process.</param>
    /// <param name="adapter">Launch options.</param>
    /// <returns>Processed arguments.</returns>
    /// <exception cref="ArgumentException">Throw if arguments was not found.</exception>
    private static string ArgumentStrProcesser(string argument, ArgumentsAdapter adapter)
    {
        var jvmArg = argument.Split('=');
        if (jvmArg.Length == 2)
        {
            var argKey = jvmArg[1];

            return ReplaceArgument(argKey, adapter, out string result) ? argument.Replace(jvmArg[1], result) : argument;
        }
        else
        {
            return ReplaceArgument(argument, adapter, out string result) ? result : argument;
        }
    }

    public static ICollection<string> GetEffectiveArguments(
        IEnumerable<ArgumentElement> argumentElements,
        ArgumentsAdapter adapter)
        ArgumentsAdapter adapter)
    {
        Collection<string> finalArgs = [];

        foreach (var element in argumentElements)
        {
            switch (element)
            {
                case StringArgument strArg:
                    finalArgs.Add(ArgumentStrProcesser(strArg.Value, adapter));
                    break;
                case RuledArgument ruledArg:
                    {
                        if (AreRulesSatisfied(ruledArg.Rules, adapter))
                        {
                            finalArgs.AddRange(ruledArg.Value.Select(arg => ArgumentStrProcesser(arg, adapter)));
                        }

                        break;
                    }
            }
        }

        return finalArgs;
    }

    public static ICollection<string> GetEffectiveArguments(
        IEnumerable<string> argumentElements,
        ArgumentsAdapter adapter)
        ArgumentsAdapter adapter)
    {
        Collection<string> finalArgs = [];

        foreach (var item in argumentElements)
        {
            if (item[0] == '$')
            {
                if (!adapter.Arguments.TryGetValue(item, out var value))
                {
                    throw new ArgumentException($"Argument '{item}' not found in Arguments");
                }

                finalArgs.Add(value);
            }
            else
            {
                finalArgs.Add(item);
            }
        }

        return finalArgs;
    }

    [GeneratedRegex(@"\$\{[^}]+\}")]
    private static partial Regex ArgumentMatchPattern();
}