using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

internal static class RuleHelpers
{
    private static readonly string[] NodeInstallCommands = ["npm ci", "npm install", "yarn install", "pnpm install", "pnpm i"];
    private static readonly string[] DotnetInstallCommands = ["dotnet restore", "dotnet build", "dotnet test"];
    private static readonly string[] PythonInstallCommands = ["pip install", "poetry install"];
    private static readonly string[] GradleInstallCommands = ["gradle build", "./gradlew build"];

    public static bool IsAction(WorkflowStep step, string actionName) =>
        step.Uses?.StartsWith(actionName + "@", StringComparison.OrdinalIgnoreCase) == true ||
        step.Uses?.Equals(actionName, StringComparison.OrdinalIgnoreCase) == true;

    public static bool HasWith(WorkflowStep step, string key) => step.With.ContainsKey(key);

    public static string? GetWith(WorkflowStep step, string key) =>
        step.With.TryGetValue(key, out var value) ? value : null;

    public static bool HasNodeInstall(WorkflowJob job) => job.Steps.Any(step => ContainsAny(step.Run, NodeInstallCommands));

    public static bool HasAnyInstall(WorkflowJob job) => job.Steps.Any(step =>
        ContainsAny(step.Run, NodeInstallCommands) ||
        ContainsAny(step.Run, DotnetInstallCommands) ||
        ContainsAny(step.Run, PythonInstallCommands) ||
        ContainsAny(step.Run, GradleInstallCommands));

    public static bool HasNodeCache(WorkflowJob job) =>
        job.Steps.Any(step => IsAction(step, "actions/setup-node") && HasWith(step, "cache")) ||
        job.Steps.Any(step => IsAction(step, "actions/cache") && IsNodeCachePath(GetWith(step, "path")));

    public static bool HasRelevantCache(WorkflowJob job) =>
        HasNodeCache(job) ||
        job.Steps.Any(step => IsAction(step, "actions/cache") && IsKnownDependencyCachePath(GetWith(step, "path")));

    public static bool IsKnownDependencyCachePath(string? path) =>
        ContainsAny(path, ["~/.npm", "~/.cache/yarn", "~/.pnpm-store", "~/.nuget/packages", "~/.cache/pip", "~/.gradle/caches"]);

    public static bool IsNodeCachePath(string? path) =>
        ContainsAny(path, ["~/.npm", "~/.cache/yarn", "~/.pnpm-store"]);

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
}
