namespace GhaCacheDoctor.Core;

public sealed record Finding(
    string RuleId,
    Severity Severity,
    string Category,
    string Message,
    string? Recommendation,
    string FilePath,
    int? Line,
    string? JobId,
    string? StepName);

public sealed record ScanResult(
    IReadOnlyList<Finding> Findings,
    IReadOnlyList<WorkflowParseError> ParseErrors);

public sealed record WorkflowParseError(
    string FilePath,
    int? Line,
    string Message);

public sealed record WorkflowDocument(
    string FilePath,
    string? Name,
    IReadOnlyList<WorkflowJob> Jobs);

public sealed record WorkflowJob(
    string Id,
    string? Name,
    IReadOnlyList<WorkflowStep> Steps);

public sealed record WorkflowStep(
    string? Name,
    string? Uses,
    string? Run,
    IReadOnlyDictionary<string, string> With,
    int? Line);

public sealed record WorkflowParseResult(
    WorkflowDocument? Workflow,
    WorkflowParseError? ParseError);

public sealed record RepositoryContext(
    string RootPath,
    IReadOnlyList<string> Files,
    IReadOnlyList<string> LockFiles,
    IReadOnlyList<string> PackageJsonFiles,
    IReadOnlyList<string> CsprojFiles,
    IReadOnlyList<string> SolutionFiles,
    IReadOnlyList<string> Dockerfiles)
{
    public bool HasNodeHints => PackageJsonFiles.Count > 0 || LockFiles.Any(IsNodeLockFile);

    public bool LooksLikeNodeMonorepo =>
        PackageJsonFiles.Count > 1 ||
        LockFiles.Count(IsNodeLockFile) > 1 ||
        LockFiles.Any(path => IsNodeLockFile(path) && !IsRootPath(path)) ||
        LockFiles.Any(path => IsNodeLockFile(path) && (path.StartsWith("apps/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("packages/", StringComparison.OrdinalIgnoreCase)));

    public static bool IsNodeLockFile(string path)
    {
        var fileName = Path.GetFileName(path);
        return fileName.Equals("package-lock.json", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("npm-shrinkwrap.json", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("yarn.lock", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("pnpm-lock.yaml", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRootPath(string path) => !path.Contains('/', StringComparison.Ordinal);
}
