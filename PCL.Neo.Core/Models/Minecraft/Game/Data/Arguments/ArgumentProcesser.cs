using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest;
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
        if (ruleFeature == null) { return false; }

        // has value and value is true, if the rule is not satisfied, it will return false.
        return adapter.Features.TryGetValue(ruleFeature.First().Key, out var value) && value;
    }

    private static bool AreRulesSatisfied(List<Rule> rulesList, ArgumentsAdapter adapter)
    {
        if (rulesList.Count == 0)
        {
            return true; // No rules, default allow.
        }

        var overallPermission = true; // Default to allow, a rule can change this to false.

        foreach (var rule in rulesList)
        {
            if (string.IsNullOrEmpty(rule.Action)) { continue; } // Skip malformed rules

            var osConditionsMet = CheckOsRule(rule.Os);
            var featureConditionsMet = rule.Features == null || CheckFeaturesRule(rule.Features.Feature, adapter);

            // For a rule to apply its conditions, ALL its specified conditions (OS and Feature) must be met.
            var ruleCriteriaMet = osConditionsMet && featureConditionsMet;

            if (rule.Action.Equals("allow", StringComparison.OrdinalIgnoreCase))
            {
                // If an "allow" rule has conditions (OS or Feature specified) AND those conditions are NOT met,
                // then this argument is disallowed by this rule.
                var ruleHasAnyConditions = rule.Os != null || rule.Features != null;
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

#nullable disable // disable nullable warnings for this method

    private static bool TryGetTargetArgument(string argument, ArgumentsAdapter adapter, out string result)
    {
        if (argument[0] == '$')
        {
            var isSuccess = adapter.Arguments.TryGetValue(argument, out var value);
            result = value;
            return isSuccess;
        }

        result = argument;
        return false;
    }

#nullable restore

    /// <summary>
    /// Used for arguemnts pattern matching.
    /// </summary>
    /// <returns>Regex result.</returns>
    [GeneratedRegex(@"\$\{[^}]+\}")]
    private static partial Regex ArgumentMatchPattern();

    /// <summary>
    /// Process customed properties.
    /// </summary>
    /// <param name="argument">The arguments the need process.</param>
    /// <param name="adapter">Launch options.</param>
    /// <returns>Processed arguments.</returns>
    /// <exception cref="ArgumentException">Throw if arguments was not found.</exception>
    private static string ArgumentStrProcesser(string argument, ArgumentsAdapter adapter)
    {
        var regexMatchResult = ArgumentMatchPattern().Matches(argument);
        if (regexMatchResult.Count <= 0)
        {
            return argument;
        }

        var replacePattern = regexMatchResult.Select(it => it.Value).ToArray().Distinct();
        foreach (var it in replacePattern)
        {
            if (TryGetTargetArgument(it, adapter, out var value))
            {
                argument = argument.Replace(it, value);
            }
            else
            {
                throw new ArgumentException($"Argument '{it}' not found in Arguments");
            }
        }

        return argument;
    }

    /// <summary>
    /// Get effective arguments from argument elements.
    /// </summary>
    /// <param name="argumentElements">The argument elements to process.</param>
    /// <param name="adapter">The launch options adapter.</param>
    /// <returns>The effective arguments.</returns>
    public static ICollection<string> GetEffectiveArguments(
        IEnumerable<ArgumentElement> argumentElements,
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
                    if (AreRulesSatisfied(ruledArg.Rules, adapter))
                    {
                        finalArgs.AddRange(ruledArg.Value.Select(arg => ArgumentStrProcesser(arg, adapter)));
                    }

                    break;
            }
        }

        return finalArgs;
    }

    /// <summary>
    /// Get the effective arguments from a collection of argument elements.
    /// </summary>
    /// <param name="argumentElements">The collection of argument elements to process.</param>
    /// <param name="adapter">The launch options adapter.</param>
    /// <returns>The effective arguments.</returns>
    /// <exception cref="ArgumentException">Throw if an argument was not found.</exception>
    public static ICollection<string> GetEffectiveArguments(
        IEnumerable<string> argumentElements,
        ArgumentsAdapter adapter)
    {
        Collection<string> finalArgs = [];

        foreach (var item in argumentElements)
        {
            if (TryGetTargetArgument(item, adapter, out var result))
            {
                finalArgs.Add(result);
            }
            else
            {
                throw new ArgumentException($"Argument '{item}' not found in Arguments");
            }
        }

        return finalArgs;
    }
}
