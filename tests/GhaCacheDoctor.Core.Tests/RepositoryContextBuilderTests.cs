using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Core.Tests;

public sealed class RepositoryContextBuilderTests
{
    [Fact]
    public void BuildDetectsNodeAndDotnetRepositoryHints()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write("apps/web/package.json", "{}");
        directory.Write("src/App/App.csproj", "<Project />");
        directory.Write("GhaCacheDoctor.sln", string.Empty);
        directory.Write("Dockerfile", "FROM alpine");

        var context = new RepositoryContextBuilder().Build(directory.Path);

        Assert.Contains("package-lock.json", context.LockFiles);
        Assert.Contains("apps/web/package.json", context.PackageJsonFiles);
        Assert.Contains("src/App/App.csproj", context.CsprojFiles);
        Assert.Contains("GhaCacheDoctor.sln", context.SolutionFiles);
        Assert.Contains("Dockerfile", context.Dockerfiles);
        Assert.True(context.HasNodeHints);
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
