using GhaCacheDoctor.Core;
using GhaCacheDoctor.GitHubActions;
using GhaCacheDoctor.GitHubActions.Rules;
using GhaCacheDoctor.Reporters;

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

        var scanner = new WorkflowScanner(
            new GitHubActionsWorkflowParser(),
            new RepositoryContextBuilder(),
            GitHubActionsRules.CreateDefault());
        var result = scanner.Scan(parse.Options);
        var reporter = CreateReporter(parse.Options.Format);
        output.Write(reporter.Render(result));

        if (result.ParseErrors.Count > 0)
        {
            return 3;
        }

        return parse.Options.FailOn is not null &&
            result.Findings.Any(finding => finding.Severity >= parse.Options.FailOn)
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
          --strict                  Reserved for stricter future rules.
          -h, --help                Show help.

        """;
}

internal sealed record ScanArgumentParse(ScanOptions Options, string? Error);

internal static class ScanArguments
{
    public static ScanArgumentParse Parse(IReadOnlyList<string> args)
    {
        var repo = ".";
        var path = ".github/workflows";
        var format = OutputFormat.Text;
        Severity? failOn = null;
        var include = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var strict = false;

        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            switch (arg)
            {
                case "-h":
                case "--help":
                    return new ScanArgumentParse(CreateOptions(repo, path, format, failOn, include, exclude, strict), null);
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

                    break;
                case "--format":
                    if (!TryReadValue(args, ref index, out var formatValue) || !Enum.TryParse(formatValue, true, out format))
                    {
                        return Error("--format must be text or json.");
                    }

                    break;
                case "--fail-on":
                    if (!TryReadValue(args, ref index, out var failValue))
                    {
                        return Error("--fail-on must be none, info, warning, or error.");
                    }

                    if (failValue.Equals("none", StringComparison.OrdinalIgnoreCase))
                    {
                        failOn = null;
                        break;
                    }

                    if (!Enum.TryParse(failValue, true, out Severity parsedFailOn))
                    {
                        return Error("--fail-on must be none, info, warning, or error.");
                    }

                    failOn = parsedFailOn;
                    break;
                case "--include":
                    if (!TryReadValue(args, ref index, out var includeValue))
                    {
                        return Error("--include requires a comma-separated rule ID list.");
                    }

                    AddRuleIds(include, includeValue);
                    break;
                case "--exclude":
                    if (!TryReadValue(args, ref index, out var excludeValue))
                    {
                        return Error("--exclude requires a comma-separated rule ID list.");
                    }

                    AddRuleIds(exclude, excludeValue);
                    break;
                case "--strict":
                    strict = true;
                    break;
                default:
                    return Error($"Unknown option: {arg}");
            }
        }

        return new ScanArgumentParse(CreateOptions(repo, path, format, failOn, include, exclude, strict), null);
    }

    private static ScanArgumentParse Error(string error) =>
        new(CreateOptions(".", ".github/workflows", OutputFormat.Text, null, new HashSet<string>(), new HashSet<string>(), false), error);

    private static ScanOptions CreateOptions(
        string repositoryPath,
        string workflowPath,
        OutputFormat format,
        Severity? failOn,
        IReadOnlySet<string> include,
        IReadOnlySet<string> exclude,
        bool strict) =>
        new(repositoryPath, workflowPath, format, failOn, include, exclude, strict);

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
