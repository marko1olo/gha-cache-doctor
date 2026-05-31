using GhaCacheDoctor.Cli;

namespace GhaCacheDoctor.Cli.Tests;

public sealed class CliApplicationTests
{
    [Fact]
    public void ScanReturnsOneWhenFailOnThresholdMatchesFinding()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - run: npm ci
            """);
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = new CliApplication(output, error).Run(["scan", "--repo", directory.Path, "--fail-on", "info"]);

        Assert.Equal(1, exitCode);
        Assert.Contains("GHA-CACHE001", output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public void ScanSupportsJsonOutput()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--format", "json"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("\"ruleId\": \"GHA-CACHE003\"", output.ToString());
    }

    [Fact]
    public void ScanHonorsIncludeRuleFilter()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
                  - run: npm ci
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--include", "GHA-CACHE003"]);

        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("GHA-CACHE001", output.ToString());
        Assert.Contains("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void ScanHonorsExcludeRuleFilter()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--exclude", "GHA-CACHE003"]);

        Assert.Equal(0, exitCode);
        Assert.Equal("No cache issues found." + Environment.NewLine, output.ToString());
    }

    [Fact]
    public void ScanReturnsThreeForParseErrors()
    {
        using var directory = new TempDirectory();
        directory.Write(".github/workflows/ci.yml", "jobs: [");
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path]);

        Assert.Equal(3, exitCode);
        Assert.Contains("parse-error", output.ToString());
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
