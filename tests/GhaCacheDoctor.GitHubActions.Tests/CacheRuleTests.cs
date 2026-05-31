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

    private static WorkflowDocument Workflow(IReadOnlyList<WorkflowStep> steps) =>
        new("ci.yml", "CI", [new WorkflowJob("test", null, steps)]);

    private static RepositoryContext Repository(IReadOnlyList<string>? lockFiles = null) =>
        new(".", lockFiles ?? [], lockFiles ?? [], [], [], [], []);
}
