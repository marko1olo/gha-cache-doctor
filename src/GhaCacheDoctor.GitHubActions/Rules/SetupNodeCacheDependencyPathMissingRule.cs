using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class SetupNodeCacheDependencyPathMissingRule : IRule
{
    public string Id => "GHA-CACHE002";
    public string Title => "setup-node-cache-dependency-path-missing";
    public Severity DefaultSeverity => Severity.Warning;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        if (!repository.LooksLikeNodeMonorepo)
        {
            return [];
        }

        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            foreach (var step in job.Steps.Where(step =>
                RuleHelpers.IsAction(step, "actions/setup-node") &&
                RuleHelpers.HasWith(step, "cache") &&
                !RuleHelpers.HasWith(step, "cache-dependency-path")))
            {
                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "This repository looks like a monorepo, but setup-node cache has no cache-dependency-path.",
                    "Set `cache-dependency-path` to the lockfile used by this job, for example `apps/web/package-lock.json`.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
            }
        }

        return findings;
    }
}
