using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class RestoreKeysTooBroadRule : IRule
{
    public string Id => "GHA-CACHE004";
    public string Title => "restore-keys-too-broad";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "correctness";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            foreach (var step in job.Steps.Where(step => RuleHelpers.IsAction(step, "actions/cache")))
            {
                var restoreKeys = RuleHelpers.GetWith(step, "restore-keys");
                if (string.IsNullOrWhiteSpace(restoreKeys) || !IsBroad(restoreKeys))
                {
                    continue;
                }

                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "actions/cache restore-keys look broad and may restore unrelated caches.",
                    "Include runner OS, package manager, project path for monorepos, and lockfile context in restore keys.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
            }
        }

        return findings;
    }

    private static bool IsBroad(string restoreKeys)
    {
        var lines = restoreKeys.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return lines.Any(line =>
            line.Equals("${{ runner.os }}-", StringComparison.OrdinalIgnoreCase) ||
            line.Equals("npm-", StringComparison.OrdinalIgnoreCase) ||
            line.Equals("node-", StringComparison.OrdinalIgnoreCase) ||
            line.Equals("cache-", StringComparison.OrdinalIgnoreCase) ||
            (!line.Contains("${{", StringComparison.Ordinal) && line.Count(character => character == '-') <= 1));
    }
}
