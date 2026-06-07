using System.Text.Json;
using GhaCacheDoctor.Core;
using GhaCacheDoctor.Reporters;

namespace GhaCacheDoctor.Cli.Tests;

public sealed class ReporterTests
{
    [Fact]
    public void TextReporterReturnsNoIssuesMessageForEmptyResult()
    {
        var result = new ScanResult([], []);

        var output = new TextReporter().Render(result);

        Assert.Equal("No cache issues found." + Environment.NewLine, output);
    }

    [Fact]
    public void TextReporterIncludesFindingContextAndRecommendation()
    {
        var result = new ScanResult([
            new Finding(
                "GHA-CACHE001",
                Severity.Info,
                "performance",
                "actions/setup-node is used without dependency caching.",
                "Add `cache: npm`.",
                ".github/workflows/ci.yml",
                14,
                "test",
                "Setup Node")
        ], []);

        var output = new TextReporter().Render(result);

        Assert.Contains("[info] GHA-CACHE001 setup-node-cache-missing", output);
        Assert.Contains(".github/workflows/ci.yml", output);
        Assert.Contains("Job: test", output);
        Assert.Contains("Step: Setup Node", output);
        Assert.Contains("Line: 14", output);
        Assert.Contains("Recommendation: Add `cache: npm`.", output);
    }

    [Fact]
    public void TextReporterIncludesGradleRuleTitle()
    {
        var result = new ScanResult([
            new Finding(
                "GHA-CACHE006",
                Severity.Info,
                "performance",
                "Gradle cache missing.",
                "Add `cache: gradle`.",
                ".github/workflows/ci.yml",
                14,
                "test",
                "Build")
        ], []);

        var output = new TextReporter().Render(result);

        Assert.Contains("[info] GHA-CACHE006 gradle-cache-missing", output);
    }

    [Fact]
    public void TextReporterIncludesParseErrors()
    {
        var result = new ScanResult([], [
            new WorkflowParseError(".github/workflows/bad.yml", 3, "Invalid YAML")
        ]);

        var output = new TextReporter().Render(result);

        Assert.Contains("[error] parse-error .github/workflows/bad.yml", output);
        Assert.Contains("Line: 3", output);
        Assert.Contains("Invalid YAML", output);
    }

    [Fact]
    public void JsonReporterUsesCamelCaseAndStringSeverity()
    {
        var result = new ScanResult([
            new Finding(
                "GHA-CACHE003",
                Severity.Warning,
                "correctness",
                "Weak cache key.",
                "Use hashFiles.",
                ".github/workflows/ci.yml",
                8,
                "test",
                "Cache npm")
        ], []);

        var output = new JsonReporter().Render(result);
        using var document = JsonDocument.Parse(output);
        var finding = document.RootElement.GetProperty("findings")[0];

        Assert.Equal("GHA-CACHE003", finding.GetProperty("ruleId").GetString());
        Assert.Equal("warning", finding.GetProperty("severity").GetString());
        Assert.Equal("correctness", finding.GetProperty("category").GetString());
        Assert.Equal(".github/workflows/ci.yml", finding.GetProperty("filePath").GetString());
        Assert.Equal(8, finding.GetProperty("line").GetInt32());
        Assert.Equal("Weak cache key.", finding.GetProperty("message").GetString());
        Assert.Equal("Use hashFiles.", finding.GetProperty("recommendation").GetString());
        Assert.Equal("Cache npm", finding.GetProperty("stepName").GetString());
        Assert.True(document.RootElement.TryGetProperty("parseErrors", out _));
    }

    [Fact]
    public void GitHubSummaryReporterReturnsMarkdownForEmptyResult()
    {
        var result = new ScanResult([], []);

        var output = new GitHubSummaryReporter().Render(result);

        Assert.Contains("# gha-cache-doctor summary", output);
        Assert.Contains("| Parse errors | 0 |", output);
        Assert.Contains("No cache issues found.", output);
    }

    [Fact]
    public void GitHubSummaryReporterIncludesFindingTableAndEscapesCells()
    {
        var result = new ScanResult([
            new Finding(
                "GHA-CACHE003",
                Severity.Warning,
                "correctness",
                "Weak cache key | missing lockfile.",
                "Use hashFiles.\nKeep restore keys scoped.",
                ".github/workflows/ci.yml",
                8,
                "test",
                "Cache npm")
        ], [
            new WorkflowParseError(".github/workflows/bad.yml", 3, "Invalid | YAML")
        ]);

        var output = new GitHubSummaryReporter().Render(result);

        Assert.Contains("| Warnings | 1 |", output);
        Assert.Contains("| Warning | GHA-CACHE003 | .github/workflows/ci.yml:8 | test | Cache npm | Weak cache key \\| missing lockfile. | Use hashFiles.<br>Keep restore keys scoped. |", output);
        Assert.Contains("## Parse errors", output);
        Assert.Contains("| .github/workflows/bad.yml:3 | Invalid \\| YAML |", output);
    }
}
