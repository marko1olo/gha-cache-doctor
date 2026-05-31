using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class ActionsCacheKeyMissingLockfileHashRule : IRule
{
    public string Id => "GHA-CACHE003";
    public string Title => "actions-cache-key-missing-lockfile-hash";
    public Severity DefaultSeverity => Severity.Warning;
    public string Category => "correctness";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            foreach (var step in job.Steps.Where(step => RuleHelpers.IsAction(step, "actions/cache")))
            {
                var path = RuleHelpers.GetWith(step, "path");
                var key = RuleHelpers.GetWith(step, "key");
                if (!RuleHelpers.IsKnownDependencyCachePath(path) || RuleHelpers.ContainsLockfileSignal(key))
                {
                    continue;
                }

                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "actions/cache uses a dependency cache path, but the key does not include a lockfile hash.",
                    "Include a dependency lockfile hash, for example `${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}`.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
            }
        }

        return findings;
    }
}
