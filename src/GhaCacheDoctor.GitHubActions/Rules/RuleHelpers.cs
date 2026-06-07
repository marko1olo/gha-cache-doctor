using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

internal static class RuleHelpers
{
    private static readonly string[] NodeInstallCommands = ["npm ci", "npm install", "yarn install", "pnpm install", "pnpm i"];
    private static readonly string[] DotnetInstallCommands = ["dotnet restore", "dotnet build", "dotnet test"];
    private static readonly string[] PythonInstallCommands = ["pip install", "poetry install"];
    private static readonly string[] PipInstallCommands = ["pip install"];
    private static readonly string[] GradleDependencyCachePaths = ["~/.gradle/caches"];
    private static readonly string[] GradleCommands = ["./gradlew", "gradlew", "gradle"];
    private static readonly string[] GradleDependencyTasks = ["build", "test"];

    public static bool IsAction(WorkflowStep step, string actionName) =>
        step.Uses?.StartsWith(actionName + "@", StringComparison.OrdinalIgnoreCase) == true ||
        step.Uses?.Equals(actionName, StringComparison.OrdinalIgnoreCase) == true;

    public static bool HasWith(WorkflowStep step, string key) => step.With.ContainsKey(key);

    public static string? GetWith(WorkflowStep step, string key) =>
        step.With.TryGetValue(key, out var value) ? value : null;

    public static bool HasNodeInstall(WorkflowJob job) => job.Steps.Any(step => ContainsAny(step.Run, NodeInstallCommands));

    public static bool HasPythonInstall(WorkflowJob job) => job.Steps.Any(step => ContainsAny(step.Run, PythonInstallCommands));

    public static bool HasPipInstall(WorkflowJob job) => job.Steps.Any(step => ContainsAny(step.Run, PipInstallCommands));

    public static bool IsPipInstall(WorkflowStep step) => ContainsAny(step.Run, PipInstallCommands);

    public static bool HasSetupPython(WorkflowJob job) => job.Steps.Any(step => IsAction(step, "actions/setup-python"));

    public static bool HasPythonHints(RepositoryContext repository) =>
        repository.PythonProjectFiles.Count > 0 || repository.LockFiles.Any(IsPythonLockFile);

    public static bool HasAnyInstall(WorkflowJob job) => job.Steps.Any(step =>
        ContainsAny(step.Run, NodeInstallCommands) ||
        ContainsAny(step.Run, DotnetInstallCommands) ||
        ContainsAny(step.Run, PythonInstallCommands) ||
        HasGradleBuildOrTest(step));

    public static bool HasNodeCache(WorkflowJob job) =>
        job.Steps.Any(step => IsAction(step, "actions/setup-node") && HasWith(step, "cache")) ||
        job.Steps.Any(step => IsAction(step, "actions/cache") && IsNodeCachePath(GetWith(step, "path")));

    public static bool HasPythonCache(WorkflowJob job) =>
        job.Steps.Any(step => IsAction(step, "actions/setup-python") && IsPipCache(GetWith(step, "cache"))) ||
        job.Steps.Any(step => IsAction(step, "actions/cache") && IsPythonCachePath(GetWith(step, "path")));

    public static bool HasRelevantCache(WorkflowJob job) =>
        HasNodeCache(job) ||
        HasPythonCache(job) ||
        job.Steps.Any(step => IsAction(step, "actions/cache") && IsKnownDependencyCachePath(GetWith(step, "path")));

    public static bool HasGradleCache(WorkflowJob job) =>
        HasGradleCache(job.Steps);

    public static bool HasGradleCache(IEnumerable<WorkflowStep> steps) =>
        steps.Any(step => IsAction(step, "actions/setup-java") &&
            GetWith(step, "cache")?.Trim().Equals("gradle", StringComparison.OrdinalIgnoreCase) == true) ||
        steps.Any(step => IsAction(step, "gradle/actions/setup-gradle")) ||
        steps.Any(step => IsAction(step, "actions/cache") && IsGradleCachePath(GetWith(step, "path")));

    public static bool HasGradleBuildOrTest(WorkflowStep step)
    {
        if (string.IsNullOrWhiteSpace(step.Run))
        {
            return false;
        }

        var lines = step.Run.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return lines.Any(ContainsGradleDependencyTask);
    }

    public static bool IsKnownDependencyCachePath(string? path) =>
        ContainsAny(path, ["~/.npm", "~/.cache/yarn", "~/.pnpm-store", "~/.nuget/packages", "~/.cache/pip", "~/.gradle/caches"]);

    public static bool IsGradleCachePath(string? path) => ContainsAny(path, GradleDependencyCachePaths);

    public static bool IsNodeCachePath(string? path) =>
        ContainsAny(path, ["~/.npm", "~/.cache/yarn", "~/.pnpm-store"]);

    public static bool IsPythonCachePath(string? path) =>
        ContainsAny(path, ["~/.cache/pip"]);

    public static bool IsPipCache(string? cache) =>
        cache?.Equals("pip", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsPythonLockFile(string path)
    {
        var fileName = Path.GetFileName(path);
        return fileName.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("poetry.lock", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("Pipfile.lock", StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsLockfileSignal(string? value) =>
        ContainsAny(value, ["hashFiles(", "package-lock.json", "npm-shrinkwrap.json", "yarn.lock", "pnpm-lock.yaml", "packages.lock.json", "requirements.txt", "poetry.lock", "Pipfile.lock", "gradle.lockfile"]);

    public static string DetectNodeCacheKind(RepositoryContext repository, WorkflowJob job)
    {
        if (repository.LockFiles.Any(path => Path.GetFileName(path).Equals("pnpm-lock.yaml", StringComparison.OrdinalIgnoreCase)) ||
            job.Steps.Any(step => ContainsAny(step.Run, ["pnpm install", "pnpm i"])))
        {
            return "pnpm";
        }

        if (repository.LockFiles.Any(path => Path.GetFileName(path).Equals("yarn.lock", StringComparison.OrdinalIgnoreCase)) ||
            job.Steps.Any(step => ContainsAny(step.Run, ["yarn install"])))
        {
            return "yarn";
        }

        return "npm";
    }

    public static bool ContainsAny(string? value, IEnumerable<string> needles) =>
        value is not null && needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private static bool ContainsGradleDependencyTask(string line)
    {
        foreach (var command in GradleCommands)
        {
            var searchIndex = 0;
            while (searchIndex < line.Length)
            {
                var index = line.IndexOf(command, searchIndex, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    break;
                }

                searchIndex = index + command.Length;

                if (!IsCommandBoundary(line, index, command.Length))
                {
                    continue;
                }

                var suffix = line[(index + command.Length)..];
                var tokens = suffix.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tokens.Any(IsGradleDependencyTask))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsCommandBoundary(string line, int index, int length)
    {
        var prefix = line[..index].TrimEnd();
        var hasValidPrefix = prefix.Length == 0 ||
            prefix.EndsWith("&&", StringComparison.Ordinal) ||
            prefix.EndsWith("||", StringComparison.Ordinal) ||
            prefix.EndsWith(';') ||
            prefix.EndsWith('(');
        var suffixIndex = index + length;
        var hasValidSuffix = suffixIndex == line.Length || char.IsWhiteSpace(line[suffixIndex]);

        return hasValidPrefix && hasValidSuffix;
    }

    private static bool IsGradleDependencyTask(string token)
    {
        var task = token.Split(':', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? token;
        return GradleDependencyTasks.Contains(task, StringComparer.OrdinalIgnoreCase);
    }
}
