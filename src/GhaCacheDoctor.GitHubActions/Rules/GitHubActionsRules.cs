using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public static class GitHubActionsRules
{
    public static IReadOnlyList<IRule> CreateDefault() =>
    [
        new SetupNodeCacheMissingRule(),
        new SetupNodeCacheDependencyPathMissingRule(),
        new ActionsCacheKeyMissingLockfileHashRule(),
        new RestoreKeysTooBroadRule(),
        new InstallStepWithoutCacheRule(),
        new SetupPythonPipCacheMissingRule()
    ];
}
