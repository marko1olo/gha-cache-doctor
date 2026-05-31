namespace GhaCacheDoctor.Core;

public sealed class RepositoryContextBuilder
{
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        ".idea",
        "bin",
        "obj",
        "node_modules"
    };

    public RepositoryContext Build(string rootPath)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        var files = Directory.Exists(fullRoot)
            ? Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
                .Where(file => !IsIgnored(fullRoot, file))
                .Select(file => Normalize(Path.GetRelativePath(fullRoot, file)))
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : [];

        return new RepositoryContext(
            fullRoot,
            files,
            files.Where(IsLockFile).ToArray(),
            files.Where(path => Path.GetFileName(path).Equals("package.json", StringComparison.OrdinalIgnoreCase)).ToArray(),
            files.Where(path => path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase)).ToArray(),
            files.Where(path => path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase)).ToArray(),
            files.Where(path => Path.GetFileName(path).Equals("Dockerfile", StringComparison.OrdinalIgnoreCase)).ToArray());
    }

    private static bool IsIgnored(string rootPath, string filePath)
    {
        var relative = Normalize(Path.GetRelativePath(rootPath, filePath));
        return relative.Split('/').Any(part => IgnoredDirectories.Contains(part));
    }

    private static bool IsLockFile(string path)
    {
        var fileName = Path.GetFileName(path);
        return fileName.Equals("package-lock.json", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("npm-shrinkwrap.json", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("yarn.lock", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("pnpm-lock.yaml", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("packages.lock.json", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("poetry.lock", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("Pipfile.lock", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("gradle.lockfile", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string path) => path.Replace('\\', '/');
}
