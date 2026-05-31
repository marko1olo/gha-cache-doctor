using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class SetupNodeCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE001";
    public string Title => "setup-node-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            if (!RuleHelpers.HasNodeInstall(job) && !repository.HasNodeHints)
            {
                continue;
            }

            if (RuleHelpers.HasNodeCache(job))
            {
                continue;
            }

            foreach (var step in job.Steps.Where(step => RuleHelpers.IsAction(step, "actions/setup-node") && !RuleHelpers.HasWith(step, "cache")))
            {
                var cacheKind = RuleHelpers.DetectNodeCacheKind(repository, job);
                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "actions/setup-node is used without dependency caching.",
                    $"Add `cache: {cacheKind}` to the setup-node `with` block.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
            }
        }

        return findings;
    }
}
