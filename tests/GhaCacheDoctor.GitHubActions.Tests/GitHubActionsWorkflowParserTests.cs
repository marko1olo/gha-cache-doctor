using GhaCacheDoctor.GitHubActions;

namespace GhaCacheDoctor.GitHubActions.Tests;

public sealed class GitHubActionsWorkflowParserTests
{
    [Fact]
    public void ParseReadsJobsStepsAndWithValues()
    {
        using var directory = new TempDirectory();
        var workflow = directory.Write(
            ".github/workflows/ci.yml",
            """
            name: CI
            on: [push]
            jobs:
              test:
                name: Test
                steps:
                  - name: Setup Node
                    uses: actions/setup-node@v4
                    with:
                      node-version: 22
                  - name: Install
                    run: npm ci
            """);

        var result = new GitHubActionsWorkflowParser().Parse(workflow);

        Assert.Null(result.ParseError);
        Assert.NotNull(result.Workflow);
        Assert.Equal("CI", result.Workflow.Name);
        var job = Assert.Single(result.Workflow.Jobs);
        Assert.Equal("test", job.Id);
        Assert.Equal("Test", job.Name);
        Assert.Equal("actions/setup-node@v4", job.Steps[0].Uses);
        Assert.Equal("22", job.Steps[0].With["node-version"]);
        Assert.Equal("npm ci", job.Steps[1].Run);
    }

    [Fact]
    public void ParseReturnsParseErrorForInvalidYaml()
    {
        using var directory = new TempDirectory();
        var workflow = directory.Write(".github/workflows/ci.yml", "jobs: [");

        var result = new GitHubActionsWorkflowParser().Parse(workflow);

        Assert.Null(result.Workflow);
        Assert.NotNull(result.ParseError);
    }
}

internal sealed class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gha-cache-doctor-" + Guid.NewGuid());
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public string Write(string relativePath, string contents)
    {
        var filePath = System.IO.Path.Combine(Path, relativePath);
        var parent = System.IO.Path.GetDirectoryName(filePath);
        if (parent is not null)
        {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllText(filePath, contents);
        return filePath;
    }

    public void Dispose()
    {
        Directory.Delete(Path, recursive: true);
    }
}
