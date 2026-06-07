using GhaCacheDoctor.Core;
using GhaCacheDoctor.GitHubActions.Rules;

namespace GhaCacheDoctor.GitHubActions.Tests;

public sealed class CacheRuleTests
{
    [Fact]
    public void SetupNodeCacheMissingReportsWhenInstallUsesNode()
    {
        var workflow = Workflow([
            new("Setup Node", "actions/setup-node@v4", null, new Dictionary<string, string>(), 10),
            new("Install", null, "npm ci", new Dictionary<string, string>(), 12)
        ]);
        var repository = Repository(lockFiles: ["package-lock.json"]);

        var finding = Assert.Single(new SetupNodeCacheMissingRule().Analyze(workflow, repository));

        Assert.Equal("GHA-CACHE001", finding.RuleId);
        Assert.Contains("cache: npm", finding.Recommendation);
    }

    [Fact]
    public void SetupNodeCacheMissingDoesNotReportWhenSetupNodeHasCache()
    {
        var workflow = Workflow([
            new("Setup Node", "actions/setup-node@v4", null, new Dictionary<string, string> { ["cache"] = "npm" }, 10),
            new("Install", null, "npm ci", new Dictionary<string, string>(), 12)
        ]);

        var findings = new SetupNodeCacheMissingRule().Analyze(workflow, Repository(lockFiles: ["package-lock.json"]));

        Assert.Empty(findings);
    }

    [Fact]
    public void SetupNodeCacheDependencyPathMissingReportsForMonorepo()
    {
        var workflow = Workflow([
            new("Setup Node", "actions/setup-node@v4", null, new Dictionary<string, string> { ["cache"] = "npm" }, 10)
        ]);
        var repository = Repository(lockFiles: ["apps/web/package-lock.json", "apps/api/package-lock.json"]);

        var finding = Assert.Single(new SetupNodeCacheDependencyPathMissingRule().Analyze(workflow, repository));

        Assert.Equal("GHA-CACHE002", finding.RuleId);
    }

    [Fact]
    public void SetupNodeCacheDependencyPathMissingReportsForPnpmWorkspace()
    {
        var workflow = Workflow([
            new("Setup Node", "actions/setup-node@v4", null, new Dictionary<string, string> { ["cache"] = "pnpm" }, 10)
        ]);
        var repository = Repository(
            files: ["pnpm-workspace.yaml", "package.json", "apps/web/package.json", "pnpm-lock.yaml"],
            lockFiles: ["pnpm-lock.yaml"],
            packageJsonFiles: ["package.json", "apps/web/package.json"]);

        var finding = Assert.Single(new SetupNodeCacheDependencyPathMissingRule().Analyze(workflow, repository));

        Assert.Equal("GHA-CACHE002", finding.RuleId);
        Assert.Contains("pnpm-lock.yaml", finding.Recommendation);
    }

    [Fact]
    public void SetupNodeCacheDependencyPathMissingAllowsPnpmWorkspaceDependencyPath()
    {
        var workflow = Workflow([
            new("Setup Node", "actions/setup-node@v4", null, new Dictionary<string, string>
            {
                ["cache"] = "pnpm",
                ["cache-dependency-path"] = "pnpm-lock.yaml\napps/*/pnpm-lock.yaml"
            }, 10)
        ]);
        var repository = Repository(
            files: ["pnpm-workspace.yaml", "package.json", "apps/web/package.json", "pnpm-lock.yaml"],
            lockFiles: ["pnpm-lock.yaml"],
            packageJsonFiles: ["package.json", "apps/web/package.json"]);

        var findings = new SetupNodeCacheDependencyPathMissingRule().Analyze(workflow, repository);

        Assert.Empty(findings);
    }

    [Fact]
    public void ActionsCacheKeyMissingLockfileHashReportsDependencyCacheKey()
    {
        var workflow = Workflow([
            new("Cache npm", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["path"] = "~/.npm",
                ["key"] = "npm-cache"
            }, 5)
        ]);

        var finding = Assert.Single(new ActionsCacheKeyMissingLockfileHashRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE003", finding.RuleId);
    }

    [Fact]
    public void ActionsCacheKeyMissingLockfileHashAllowsHashFiles()
    {
        var workflow = Workflow([
            new("Cache npm", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["path"] = "~/.npm",
                ["key"] = "${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}"
            }, 5)
        ]);

        var findings = new ActionsCacheKeyMissingLockfileHashRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void RestoreKeysTooBroadReportsStaticPrefix()
    {
        var workflow = Workflow([
            new("Cache npm", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["path"] = "~/.npm",
                ["key"] = "${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}",
                ["restore-keys"] = "npm-"
            }, 5)
        ]);

        var finding = Assert.Single(new RestoreKeysTooBroadRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE004", finding.RuleId);
        Assert.Equal(Severity.Info, finding.Severity);
    }

    [Fact]
    public void RestoreKeysTooBroadReportsRunnerOsPrefix()
    {
        var workflow = Workflow([
            new("Cache npm", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["restore-keys"] = "${{ runner.os }}-"
            }, 5)
        ]);

        var finding = Assert.Single(new RestoreKeysTooBroadRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE004", finding.RuleId);
    }

    [Fact]
    public void RestoreKeysTooBroadAllowsContextualPrefix()
    {
        var workflow = Workflow([
            new("Cache npm", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["path"] = "~/.npm",
                ["key"] = "${{ runner.os }}-npm-apps-web-${{ hashFiles('apps/web/package-lock.json') }}",
                ["restore-keys"] = "${{ runner.os }}-npm-apps-web-"
            }, 5)
        ]);

        var findings = new RestoreKeysTooBroadRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void RestoreKeysTooBroadHandlesMissingRestoreKeys()
    {
        var workflow = Workflow([
            new("Cache npm", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["path"] = "~/.npm",
                ["key"] = "npm-cache"
            }, 5)
        ]);

        var findings = new RestoreKeysTooBroadRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void RestoreKeysTooBroadReportsAdditionalPatternsInStrictMode()
    {
        var workflow = Workflow([
            new("Cache pnpm", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["restore-keys"] = "pnpm-"
            }, 5)
        ]);

        var normalFindings = new RestoreKeysTooBroadRule().Analyze(workflow, Repository());
        var strictFinding = Assert.Single(new RestoreKeysTooBroadRule().Analyze(workflow, Repository(), strictMode: true));

        Assert.Empty(normalFindings);
        Assert.Equal("GHA-CACHE004", strictFinding.RuleId);
    }

    [Fact]
    public void InstallStepWithoutCacheReportsDependencyInstall()
    {
        var workflow = Workflow([
            new("Install", null, "dotnet restore", new Dictionary<string, string>(), 12)
        ]);

        var finding = Assert.Single(new InstallStepWithoutCacheRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE005", finding.RuleId);
        Assert.Equal("test", finding.JobId);
    }

    [Fact]
    public void InstallStepWithoutCacheReportsNpmCiWithoutCache()
    {
        var workflow = Workflow([
            new("Install", null, "npm ci", new Dictionary<string, string>(), 12)
        ]);

        var finding = Assert.Single(new InstallStepWithoutCacheRule().Analyze(workflow, Repository(lockFiles: ["package-lock.json"])));

        Assert.Equal("GHA-CACHE005", finding.RuleId);
    }

    [Fact]
    public void InstallStepWithoutCacheAllowsSetupNodeCache()
    {
        var workflow = Workflow([
            new("Setup Node", "actions/setup-node@v4", null, new Dictionary<string, string> { ["cache"] = "npm" }, 5),
            new("Install", null, "npm ci", new Dictionary<string, string>(), 12)
        ]);

        var findings = new InstallStepWithoutCacheRule().Analyze(workflow, Repository(lockFiles: ["package-lock.json"]));

        Assert.Empty(findings);
    }

    [Fact]
    public void InstallStepWithoutCacheAllowsMatchingCache()
    {
        var workflow = Workflow([
            new("Cache NuGet", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["path"] = "~/.nuget/packages",
                ["key"] = "${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}"
            }, 5),
            new("Restore", null, "dotnet restore", new Dictionary<string, string>(), 12)
        ]);

        var findings = new InstallStepWithoutCacheRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void InstallStepWithoutCacheStrictModeReportsLikelyNpmInstall()
    {
        var workflow = Workflow([
            new("Install", null, "npm install", new Dictionary<string, string>(), 12)
        ]);
        var repository = Repository(lockFiles: ["package-lock.json"], packageJsonFiles: ["package.json"]);

        var normalFindings = new InstallStepWithoutCacheRule().Analyze(workflow, repository);
        var strictFinding = Assert.Single(new InstallStepWithoutCacheRule().Analyze(workflow, repository, strictMode: true));

        Assert.Empty(normalFindings);
        Assert.Equal("GHA-CACHE005", strictFinding.RuleId);
    }

    [Fact]
    public void InstallStepWithoutCacheSkipsExplicitNoCacheInstall()
    {
        var workflow = Workflow([
            new("Install", null, "pip install --no-cache-dir -r requirements.txt", new Dictionary<string, string>(), 12)
        ]);

        var findings = new InstallStepWithoutCacheRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void InstallStepWithoutCacheDoesNotDuplicateGradleSpecificRule()
    {
        var workflow = Workflow([
            new("Build", null, "./gradlew build", new Dictionary<string, string>(), 12)
        ]);

        var findings = new InstallStepWithoutCacheRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void GradleCacheMissingReportsGradleBuildWithoutCache()
    {
        var workflow = Workflow([
            new("Build", null, "./gradlew build", new Dictionary<string, string>(), 12)
        ]);

        var finding = Assert.Single(new GradleCacheMissingRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE006", finding.RuleId);
        Assert.Equal("test", finding.JobId);
        Assert.Equal("Build", finding.StepName);
    }

    [Fact]
    public void GradleCacheMissingReportsGradleTestCommand()
    {
        var workflow = Workflow([
            new("Test", null, "cd app && gradle --no-daemon test", new Dictionary<string, string>(), 12)
        ]);

        var finding = Assert.Single(new GradleCacheMissingRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE006", finding.RuleId);
    }

    [Fact]
    public void GradleCacheMissingReportsQualifiedGradleTask()
    {
        var workflow = Workflow([
            new("Build app", null, "./gradlew --no-daemon :app:build", new Dictionary<string, string>(), 12)
        ]);

        var finding = Assert.Single(new GradleCacheMissingRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE006", finding.RuleId);
    }

    [Fact]
    public void GradleCacheMissingAllowsSetupJavaGradleCache()
    {
        var workflow = Workflow([
            new("Setup Java", "actions/setup-java@v4", null, new Dictionary<string, string>
            {
                ["distribution"] = "temurin",
                ["java-version"] = "21",
                ["cache"] = "gradle"
            }, 6),
            new("Build", null, "./gradlew build", new Dictionary<string, string>(), 12)
        ]);

        var findings = new GradleCacheMissingRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void GradleCacheMissingAllowsSetupGradleAction()
    {
        var workflow = Workflow([
            new("Setup Gradle", "gradle/actions/setup-gradle@v4", null, new Dictionary<string, string>(), 6),
            new("Build", null, "./gradlew build", new Dictionary<string, string>(), 12)
        ]);

        var findings = new GradleCacheMissingRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void GradleCacheMissingAllowsManualGradleCache()
    {
        var workflow = Workflow([
            new("Cache Gradle", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["path"] = """
                    ~/.gradle/caches
                    ~/.gradle/wrapper
                    """,
                ["key"] = "${{ runner.os }}-gradle-${{ hashFiles('**/*.gradle*', '**/gradle-wrapper.properties') }}"
            }, 6),
            new("Build", null, "./gradlew build", new Dictionary<string, string>(), 12)
        ]);

        var findings = new GradleCacheMissingRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void GradleCacheMissingReportsWhenCacheRunsAfterGradle()
    {
        var workflow = Workflow([
            new("Build", null, "./gradlew build", new Dictionary<string, string>(), 12),
            new("Setup Gradle", "gradle/actions/setup-gradle@v4", null, new Dictionary<string, string>(), 16)
        ]);

        var finding = Assert.Single(new GradleCacheMissingRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE006", finding.RuleId);
        Assert.Equal("Build", finding.StepName);
    }

    [Fact]
    public void GradleCacheMissingReportsWrapperOnlyManualCache()
    {
        var workflow = Workflow([
            new("Cache Gradle Wrapper", "actions/cache@v4", null, new Dictionary<string, string>
            {
                ["path"] = "~/.gradle/wrapper",
                ["key"] = "${{ runner.os }}-gradle-wrapper-${{ hashFiles('**/gradle-wrapper.properties') }}"
            }, 6),
            new("Build", null, "./gradlew build", new Dictionary<string, string>(), 12)
        ]);

        var finding = Assert.Single(new GradleCacheMissingRule().Analyze(workflow, Repository()));

        Assert.Equal("GHA-CACHE006", finding.RuleId);
    }

    [Fact]
    public void GradleCacheMissingIgnoresEchoedGradleCommand()
    {
        var workflow = Workflow([
            new("Explain", null, "echo ./gradlew build", new Dictionary<string, string>(), 12)
        ]);

        var findings = new GradleCacheMissingRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    [Fact]
    public void GradleCacheMissingIgnoresOtherGradleTasks()
    {
        var workflow = Workflow([
            new("Show dependencies", null, "./gradlew dependencies", new Dictionary<string, string>(), 12)
        ]);

        var findings = new GradleCacheMissingRule().Analyze(workflow, Repository());

        Assert.Empty(findings);
    }

    private static WorkflowDocument Workflow(IReadOnlyList<WorkflowStep> steps) =>
        new("ci.yml", "CI", [new WorkflowJob("test", null, steps)]);

    private static RepositoryContext Repository(
        IReadOnlyList<string>? files = null,
        IReadOnlyList<string>? lockFiles = null,
        IReadOnlyList<string>? packageJsonFiles = null) =>
        new(".", files ?? (lockFiles ?? []).Concat(packageJsonFiles ?? []).ToArray(), lockFiles ?? [], packageJsonFiles ?? [], [], [], []);
}
