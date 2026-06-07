using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class GradleCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE006";
    public string Title => "gradle-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            for (var stepIndex = 0; stepIndex < job.Steps.Count; stepIndex++)
            {
                var gradleStep = job.Steps[stepIndex];
                if (!RuleHelpers.HasGradleBuildOrTest(gradleStep) ||
                    RuleHelpers.HasGradleCache(job.Steps.Take(stepIndex)))
                {
                    continue;
                }

                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "This job runs Gradle build or test tasks before configuring Gradle dependency caching.",
                    "Add Gradle caching before the Gradle step, such as `actions/setup-java` with `cache: gradle`, `gradle/actions/setup-gradle`, or `actions/cache` for `~/.gradle/caches` and `~/.gradle/wrapper`.",
                    workflow.FilePath,
                    gradleStep.Line,
                    job.Id,
                    gradleStep.Name));
                break;
            }
        }

        return findings;
    }
}
