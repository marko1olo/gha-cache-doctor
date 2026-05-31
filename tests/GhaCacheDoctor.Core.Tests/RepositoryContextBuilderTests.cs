using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Core.Tests;

public sealed class RepositoryContextBuilderTests
{
    [Fact]
    public void BuildDetectsNodeAndDotnetRepositoryHints()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write("npm-shrinkwrap.json", "{}");
        directory.Write("yarn.lock", string.Empty);
        directory.Write("pnpm-lock.yaml", string.Empty);
        directory.Write("apps/web/package.json", "{}");
        directory.Write("src/App/App.csproj", "<Project />");
        directory.Write("src/Lib/Lib.fsproj", "<Project />");
        directory.Write("GhaCacheDoctor.sln", string.Empty);
        directory.Write("global.json", "{}");
        directory.Write("packages.lock.json", "{}");
        directory.Write("Dockerfile", "FROM alpine");
        directory.Write("docker-compose.yml", "services: {}");
        directory.Write("compose.yml", "services: {}");

        var context = new RepositoryContextBuilder().Build(directory.Path);

        Assert.Contains("package-lock.json", context.LockFiles);
        Assert.Contains("npm-shrinkwrap.json", context.LockFiles);
        Assert.Contains("yarn.lock", context.LockFiles);
        Assert.Contains("pnpm-lock.yaml", context.LockFiles);
        Assert.Contains("apps/web/package.json", context.PackageJsonFiles);
        Assert.Contains("src/App/App.csproj", context.CsprojFiles);
        Assert.Contains("src/Lib/Lib.fsproj", context.CsprojFiles);
        Assert.Contains("GhaCacheDoctor.sln", context.SolutionFiles);
        Assert.Contains("global.json", context.GlobalJsonFiles);
        Assert.Contains("packages.lock.json", context.LockFiles);
        Assert.Contains("Dockerfile", context.Dockerfiles);
        Assert.Contains("docker-compose.yml", context.ComposeFiles);
        Assert.Contains("compose.yml", context.ComposeFiles);
        Assert.True(context.HasNodeHints);
    }

    [Fact]
    public void BuildDetectsPythonAndGradleRepositoryHints()
    {
        using var directory = new TempDirectory();
        directory.Write("requirements.txt", "pytest");
        directory.Write("pyproject.toml", "[project]");
        directory.Write("poetry.lock", string.Empty);
        directory.Write("Pipfile.lock", "{}");
        directory.Write("build.gradle", string.Empty);
        directory.Write("build.gradle.kts", string.Empty);
        directory.Write("gradle.lockfile", string.Empty);
        directory.Write("gradlew", string.Empty);

        var context = new RepositoryContextBuilder().Build(directory.Path);

        Assert.Contains("requirements.txt", context.LockFiles);
        Assert.Contains("pyproject.toml", context.LockFiles);
        Assert.Contains("pyproject.toml", context.PythonProjectFiles);
        Assert.Contains("poetry.lock", context.LockFiles);
        Assert.Contains("Pipfile.lock", context.LockFiles);
        Assert.Contains("build.gradle", context.GradleFiles);
        Assert.Contains("build.gradle.kts", context.GradleFiles);
        Assert.Contains("gradle.lockfile", context.GradleFiles);
        Assert.Contains("gradlew", context.GradleFiles);
    }

    [Fact]
    public void BuildDetectsNodeMonorepoHints()
    {
        using var directory = new TempDirectory();
        directory.Write("apps/web/package-lock.json", "{}");
        directory.Write("packages/ui/package-lock.json", "{}");

        var context = new RepositoryContextBuilder().Build(directory.Path);

        Assert.True(context.LooksLikeNodeMonorepo);
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

    public void Write(string relativePath, string contents)
    {
        var filePath = System.IO.Path.Combine(Path, relativePath);
        var parent = System.IO.Path.GetDirectoryName(filePath);
        if (parent is not null)
        {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllText(filePath, contents);
    }

    public void Dispose()
    {
        Directory.Delete(Path, recursive: true);
    }
}
