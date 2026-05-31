namespace GhaCacheDoctor.Core;

public sealed class WorkflowScanner
{
    private readonly IWorkflowParser parser;
    private readonly RepositoryContextBuilder repositoryContextBuilder;
    private readonly IReadOnlyList<IRule> rules;

    public WorkflowScanner(
        IWorkflowParser parser,
        RepositoryContextBuilder repositoryContextBuilder,
        IReadOnlyList<IRule> rules)
    {
        this.parser = parser;
        this.repositoryContextBuilder = repositoryContextBuilder;
        this.rules = rules;
    }

    public ScanResult Scan(ScanOptions options)
    {
        var repository = repositoryContextBuilder.Build(options.RepositoryPath);
        var workflowFiles = ResolveWorkflowFiles(options).ToArray();
        var findings = new List<Finding>();
        var parseErrors = new List<WorkflowParseError>();
        var selectedRules = rules.Where(rule => IsSelected(rule, options)).ToArray();

        foreach (var workflowFile in workflowFiles)
        {
            var parseResult = parser.Parse(workflowFile);
            if (parseResult.ParseError is not null)
            {
                parseErrors.Add(parseResult.ParseError);
                continue;
            }

            if (parseResult.Workflow is null)
            {
                continue;
            }

            foreach (var rule in selectedRules)
            {
                findings.AddRange(rule.Analyze(parseResult.Workflow, repository, options.Strict));
            }
        }

        return new ScanResult(
            findings.OrderBy(finding => finding.FilePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(finding => finding.Line ?? int.MaxValue)
                .ThenBy(finding => finding.RuleId, StringComparer.Ordinal)
                .ToArray(),
            parseErrors.ToArray());
    }

    private static bool IsSelected(IRule rule, ScanOptions options)
    {
        if (options.IncludeRuleIds.Count > 0 && !options.IncludeRuleIds.Contains(rule.Id))
        {
            return false;
        }

        return !options.ExcludeRuleIds.Contains(rule.Id);
    }

    private static IEnumerable<string> ResolveWorkflowFiles(ScanOptions options)
    {
        var workflowPath = Path.GetFullPath(Path.Combine(options.RepositoryPath, options.WorkflowPath));
        if (File.Exists(workflowPath))
        {
            yield return workflowPath;
            yield break;
        }

        if (!Directory.Exists(workflowPath))
        {
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(workflowPath, "*.yml").Concat(Directory.EnumerateFiles(workflowPath, "*.yaml"))
            .Order(StringComparer.OrdinalIgnoreCase))
        {
            yield return file;
        }
    }
}
