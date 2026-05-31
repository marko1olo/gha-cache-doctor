using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class InstallStepWithoutCacheRule : IRule
{
    public string Id => "GHA-CACHE005";
    public string Title => "install-step-without-cache";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            if (!RuleHelpers.HasAnyInstall(job) || RuleHelpers.HasRelevantCache(job))
            {
                continue;
            }

            var installStep = job.Steps.FirstOrDefault(step => RuleHelpers.ContainsAny(step.Run, ["npm ci", "npm install", "yarn install", "pnpm install", "pnpm i", "dotnet restore", "pip install", "poetry install", "gradle build", "./gradlew build"]));
            if (installStep?.Run?.Contains("--no-cache", StringComparison.OrdinalIgnoreCase) == true)
            {
                continue;
            }

            findings.Add(new Finding(
                Id,
                DefaultSeverity,
                Category,
                "This job installs dependencies but does not configure a matching dependency cache.",
                "Add a package-manager-specific cache in the same job, such as setup-node `cache` or actions/cache for the dependency store.",
                workflow.FilePath,
                installStep?.Line,
                job.Id,
                installStep?.Name));
        }

        return findings;
    }
}
