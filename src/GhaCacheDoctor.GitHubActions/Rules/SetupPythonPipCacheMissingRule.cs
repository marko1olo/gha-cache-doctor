using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class SetupPythonPipCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE007";
    public string Title => "setup-python-pip-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        if (!RuleHelpers.HasPythonHints(repository))
        {
            return [];
        }

        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            if (!RuleHelpers.HasPipInstall(job) || RuleHelpers.HasPythonCache(job))
            {
                continue;
            }

            foreach (var step in job.Steps.Where(step =>
                RuleHelpers.IsAction(step, "actions/setup-python") &&
                !RuleHelpers.IsPipCache(RuleHelpers.GetWith(step, "cache"))))
            {
                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "actions/setup-python is used without pip dependency caching.",
                    "Add `cache: pip` to the setup-python `with` block. If the dependency file is not `requirements.txt`, also set `cache-dependency-path`.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
            }
        }

        return findings;
    }
}
