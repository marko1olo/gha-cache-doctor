using GhaCacheDoctor.Core;
using GhaCacheDoctor.GitHubActions;
using GhaCacheDoctor.GitHubActions.Rules;
using GhaCacheDoctor.Reporters;
using YamlDotNet.RepresentationModel;

namespace GhaCacheDoctor.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CliApplication(Console.Out, Console.Error);
        return app.Run(args);
    }
}

public sealed class CliApplication
{
    private readonly TextWriter output;
    private readonly TextWriter error;

    public CliApplication(TextWriter output, TextWriter error)
    {
        this.output = output;
        this.error = error;
    }

    public int Run(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || args[0].Equals("scan", StringComparison.OrdinalIgnoreCase))
        {
            return RunScan(args.Count == 0 ? [] : args.Skip(1).ToArray());
        }

        if (args[0] is "-h" or "--help" or "help")
        {
            output.Write(HelpText());
            return 0;
        }

        error.WriteLine($"Unknown command: {args[0]}");
        error.Write(HelpText());
        return 2;
    }

    private int RunScan(IReadOnlyList<string> args)
    {
        var parse = ScanArguments.Parse(args);
        if (parse.Error is not null)
        {
            error.WriteLine(parse.Error);
            error.Write(HelpText());
            return 2;
        }

        var config = ScanConfigLoader.Load(parse.Arguments.RepositoryPath, parse.Arguments.ConfigPath);
        if (config.Error is not null)
        {
            error.WriteLine(config.Error);
            return 2;
        }

        var options = parse.Arguments.ToOptions(config.Config);

        var scanner = new WorkflowScanner(
            new GitHubActionsWorkflowParser(),
            new RepositoryContextBuilder(),
            GitHubActionsRules.CreateDefault());
        var result = scanner.Scan(options);
        var reporter = CreateReporter(options.Format);
        output.Write(reporter.Render(result));

        if (result.ParseErrors.Count > 0)
        {
            return 3;
        }

        return options.FailOn is not null &&
            result.Findings.Any(finding => finding.Severity >= options.FailOn)
            ? 1
            : 0;
    }

    private static IReporter CreateReporter(OutputFormat format) =>
        format is OutputFormat.Json ? new JsonReporter() : new TextReporter();

    private static string HelpText() =>
        """
        gha-cache-doctor

        Usage:
          gha-cache-doctor scan [options]

        Options:
          --repo <path>             Repository root. Defaults to current directory.
          --path <path>             Workflow file or directory. Defaults to .github/workflows.
          --format <text|json>      Output format. Defaults to text.
          --fail-on <none|info|warning|error>
          --include <ids>           Comma-separated rule IDs to include.
          --exclude <ids>           Comma-separated rule IDs to exclude.
          --strict                  Enable stricter rule behavior.
          --config <path|none>      Config file. Defaults to .gha-cache-doctor.yml if present.
          -h, --help                Show help.

        """;
}

internal sealed record ScanArgumentParse(ParsedScanArguments Arguments, string? Error);

internal sealed record ParsedScanArguments(
    string RepositoryPath,
    string? WorkflowPath,
    OutputFormat? Format,
    Severity? FailOn,
    bool FailOnSet,
    IReadOnlySet<string> IncludeRuleIds,
    bool IncludeSet,
    IReadOnlySet<string> ExcludeRuleIds,
    bool ExcludeSet,
    bool? Strict,
    string? ConfigPath)
{
    public ScanOptions ToOptions(ScanConfig config)
    {
        var include = IncludeSet ? IncludeRuleIds : config.IncludeRuleIds;
        var exclude = ExcludeSet ? ExcludeRuleIds : config.ExcludeRuleIds;
        return new ScanOptions(
            RepositoryPath,
            WorkflowPath ?? config.WorkflowPath ?? ".github/workflows",
            Format ?? config.Format ?? OutputFormat.Text,
            FailOnSet ? FailOn : config.FailOn,
            include,
            exclude,
            Strict ?? config.Strict ?? false,
            config.SeverityOverrides);
    }
}

internal static class ScanArguments
{
    public static ScanArgumentParse Parse(IReadOnlyList<string> args)
    {
        var repo = ".";
        var path = ".github/workflows";
        var format = OutputFormat.Text;
        Severity? failOn = null;
        var failOnSet = false;
        var include = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var includeSet = false;
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var excludeSet = false;
        bool? strict = null;
        string? configPath = null;
        string? workflowPath = null;
        OutputFormat? outputFormat = null;

        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            switch (arg)
            {
                case "-h":
                case "--help":
                    return new ScanArgumentParse(CreateArguments(repo, workflowPath, outputFormat, failOn, failOnSet, include, includeSet, exclude, excludeSet, strict, configPath), null);
                case "--repo":
                    if (!TryReadValue(args, ref index, out repo))
                    {
                        return Error("--repo requires a value.");
                    }

                    break;
                case "--path":
                    if (!TryReadValue(args, ref index, out path))
                    {
                        return Error("--path requires a value.");
                    }

                    workflowPath = path;
                    break;
                case "--format":
                    if (!TryReadValue(args, ref index, out var formatValue) || !Enum.TryParse(formatValue, true, out format))
                    {
                        return Error("--format must be text or json.");
                    }

                    outputFormat = format;
                    break;
                case "--fail-on":
                    if (!TryReadValue(args, ref index, out var failValue))
                    {
                        return Error("--fail-on must be none, info, warning, or error.");
                    }

                    if (failValue.Equals("none", StringComparison.OrdinalIgnoreCase))
                    {
                        failOn = null;
                        failOnSet = true;
                        break;
                    }

                    if (!Enum.TryParse(failValue, true, out Severity parsedFailOn))
                    {
                        return Error("--fail-on must be none, info, warning, or error.");
                    }

                    failOn = parsedFailOn;
                    failOnSet = true;
                    break;
                case "--include":
                    if (!TryReadValue(args, ref index, out var includeValue))
                    {
                        return Error("--include requires a comma-separated rule ID list.");
                    }

                    AddRuleIds(include, includeValue);
                    includeSet = true;
                    break;
                case "--exclude":
                    if (!TryReadValue(args, ref index, out var excludeValue))
                    {
                        return Error("--exclude requires a comma-separated rule ID list.");
                    }

                    AddRuleIds(exclude, excludeValue);
                    excludeSet = true;
                    break;
                case "--strict":
                    strict = true;
                    break;
                case "--config":
                    if (!TryReadValue(args, ref index, out configPath))
                    {
                        return Error("--config requires a file path or none.");
                    }

                    break;
                default:
                    return Error($"Unknown option: {arg}");
            }
        }

        return new ScanArgumentParse(CreateArguments(repo, workflowPath, outputFormat, failOn, failOnSet, include, includeSet, exclude, excludeSet, strict, configPath), null);
    }

    private static ScanArgumentParse Error(string error) =>
        new(CreateArguments(".", null, null, null, false, new HashSet<string>(), false, new HashSet<string>(), false, null, null), error);

    private static ParsedScanArguments CreateArguments(
        string repositoryPath,
        string? workflowPath,
        OutputFormat? format,
        Severity? failOn,
        bool failOnSet,
        IReadOnlySet<string> include,
        bool includeSet,
        IReadOnlySet<string> exclude,
        bool excludeSet,
        bool? strict,
        string? configPath) =>
        new(repositoryPath, workflowPath, format, failOn, failOnSet, include, includeSet, exclude, excludeSet, strict, configPath);

    private static bool TryReadValue(IReadOnlyList<string> args, ref int index, out string value)
    {
        if (index + 1 >= args.Count || args[index + 1].StartsWith("-", StringComparison.Ordinal))
        {
            value = string.Empty;
            return false;
        }

        index++;
        value = args[index];
        return true;
    }

    private static void AddRuleIds(HashSet<string> target, string value)
    {
        foreach (var id in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            target.Add(id);
        }
    }
}

internal sealed record ConfigLoadResult(ScanConfig Config, string? Error);

internal sealed record ScanConfig(
    string? WorkflowPath,
    OutputFormat? Format,
    Severity? FailOn,
    IReadOnlySet<string> IncludeRuleIds,
    IReadOnlySet<string> ExcludeRuleIds,
    bool? Strict,
    IReadOnlyDictionary<string, Severity> SeverityOverrides)
{
    public static ScanConfig Empty { get; } = new(
        null,
        null,
        null,
        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        null,
        new Dictionary<string, Severity>(StringComparer.OrdinalIgnoreCase));
}

internal static class ScanConfigLoader
{
    public static ConfigLoadResult Load(string repositoryPath, string? configPath)
    {
        if (configPath?.Equals("none", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new ConfigLoadResult(ScanConfig.Empty, null);
        }

        var path = ResolveConfigPath(repositoryPath, configPath);
        if (path is null)
        {
            return new ConfigLoadResult(ScanConfig.Empty, null);
        }

        if (!File.Exists(path))
        {
            return new ConfigLoadResult(ScanConfig.Empty, $"Config file not found: {path}");
        }

        try
        {
            using var reader = File.OpenText(path);
            var yaml = new YamlStream();
            yaml.Load(reader);
            if (yaml.Documents.Count == 0 || yaml.Documents[0].RootNode is not YamlMappingNode root)
            {
                return new ConfigLoadResult(ScanConfig.Empty, "Config file must contain a YAML mapping.");
            }

            return new ConfigLoadResult(Parse(root), null);
        }
        catch (Exception exception)
        {
            return new ConfigLoadResult(ScanConfig.Empty, $"Config file could not be parsed: {exception.Message}");
        }
    }

    private static string? ResolveConfigPath(string repositoryPath, string? configPath)
    {
        var root = Path.GetFullPath(repositoryPath);
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            return Path.GetFullPath(Path.IsPathRooted(configPath) ? configPath : Path.Combine(root, configPath));
        }

        var yml = Path.Combine(root, ".gha-cache-doctor.yml");
        if (File.Exists(yml))
        {
            return yml;
        }

        var yaml = Path.Combine(root, ".gha-cache-doctor.yaml");
        return File.Exists(yaml) ? yaml : null;
    }

    private static ScanConfig Parse(YamlMappingNode root)
    {
        string? workflowPath = null;
        OutputFormat? format = null;
        Severity? failOn = null;
        bool? strict = null;
        var include = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var severity = new Dictionary<string, Severity>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in root.Children)
        {
            var key = ((YamlScalarNode)entry.Key).Value;
            switch (key)
            {
                case "path":
                case "workflowPath":
                    workflowPath = Scalar(entry.Value);
                    break;
                case "format":
                    format = ParseEnum<OutputFormat>(Scalar(entry.Value), "format");
                    break;
                case "failOn":
                case "fail-on":
                    failOn = ParseFailOn(Scalar(entry.Value));
                    break;
                case "strict":
                    strict = ParseBool(Scalar(entry.Value), "strict");
                    break;
                case "include":
                    AddSequence(include, entry.Value);
                    break;
                case "exclude":
                case "disabledRules":
                    AddSequence(exclude, entry.Value);
                    break;
                case "severity":
                case "severityOverrides":
                    AddSeverityOverrides(severity, entry.Value);
                    break;
            }
        }

        return new ScanConfig(workflowPath, format, failOn, include, exclude, strict, severity);
    }

    private static string Scalar(YamlNode node) =>
        node is YamlScalarNode scalar && scalar.Value is not null
            ? scalar.Value
            : throw new InvalidOperationException("Config value must be a scalar.");

    private static T ParseEnum<T>(string value, string name)
        where T : struct =>
        Enum.TryParse<T>(value, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Invalid {name} value: {value}.");

    private static Severity? ParseFailOn(string value) =>
        value.Equals("none", StringComparison.OrdinalIgnoreCase)
            ? null
            : ParseEnum<Severity>(value, "failOn");

    private static bool ParseBool(string value, string name) =>
        bool.TryParse(value, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Invalid {name} value: {value}.");

    private static void AddSequence(HashSet<string> target, YamlNode node)
    {
        if (node is YamlSequenceNode sequence)
        {
            foreach (var child in sequence.Children)
            {
                target.Add(Scalar(child));
            }

            return;
        }

        foreach (var item in Scalar(node).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            target.Add(item);
        }
    }

    private static void AddSeverityOverrides(Dictionary<string, Severity> target, YamlNode node)
    {
        if (node is not YamlMappingNode mapping)
        {
            throw new InvalidOperationException("severity must be a mapping of rule IDs to severities.");
        }

        foreach (var entry in mapping.Children)
        {
            target[Scalar(entry.Key)] = ParseEnum<Severity>(Scalar(entry.Value), "severity");
        }
    }
}
