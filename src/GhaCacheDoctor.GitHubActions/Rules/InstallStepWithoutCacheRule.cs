using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class InstallStepWithoutCacheRule : IRule
{
    public string Id => "GHA-CACHE005";
    public string Title => "install-step-without-cache";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            var installStep = job.Steps.FirstOrDefault(step => IsInstallStep(step, repository, strictMode));
            if (installStep is null || RuleHelpers.HasRelevantCache(job))
            {
                continue;
            }

            if (installStep.Run?.Contains("--no-cache", StringComparison.OrdinalIgnoreCase) == true)
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

    private static bool IsInstallStep(WorkflowStep step, RepositoryContext repository, bool strictMode)
    {
        if (RuleHelpers.ContainsAny(step.Run, ["npm ci", "yarn install", "pnpm install", "dotnet restore", "poetry install", "gradle build", "./gradlew build"]))
        {
            return true;
        }

        if (!strictMode)
        {
            return false;
        }

        return RuleHelpers.ContainsAny(step.Run, ["npm install", "pnpm i"]) && repository.HasNodeHints ||
            RuleHelpers.ContainsAny(step.Run, ["dotnet build", "dotnet test"]) && (repository.CsprojFiles.Count > 0 || repository.SolutionFiles.Count > 0) ||
            RuleHelpers.ContainsAny(step.Run, ["pip install"]) && (repository.LockFiles.Any(path => Path.GetFileName(path).Equals("requirements.txt", StringComparison.OrdinalIgnoreCase)) || repository.PythonProjectFiles.Count > 0);
    }
}
