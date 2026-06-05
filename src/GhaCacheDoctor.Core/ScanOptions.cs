namespace GhaCacheDoctor.Core;

public sealed record ScanOptions(
    string RepositoryPath,
    string WorkflowPath,
    OutputFormat Format,
    Severity? FailOn,
    IReadOnlySet<string> IncludeRuleIds,
    IReadOnlySet<string> ExcludeRuleIds,
    bool Strict,
    IReadOnlyDictionary<string, Severity> SeverityOverrides);

public enum OutputFormat
{
    Text,
    Json
}
