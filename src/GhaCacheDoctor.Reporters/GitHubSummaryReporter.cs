using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Reporters;

public sealed class GitHubSummaryReporter : IReporter
{
    public string Render(ScanResult result)
    {
        var writer = new StringWriter();
        writer.WriteLine("# gha-cache-doctor summary");
        writer.WriteLine();

        WriteSummary(writer, result);
        WriteFindings(writer, result.Findings);
        WriteParseErrors(writer, result.ParseErrors);

        return writer.ToString();
    }

    private static void WriteSummary(StringWriter writer, ScanResult result)
    {
        writer.WriteLine("## Summary");
        writer.WriteLine();
        writer.WriteLine("| Type | Count |");
        writer.WriteLine("| --- | ---: |");
        writer.WriteLine($"| Errors | {result.Findings.Count(finding => finding.Severity == Severity.Error)} |");
        writer.WriteLine($"| Warnings | {result.Findings.Count(finding => finding.Severity == Severity.Warning)} |");
        writer.WriteLine($"| Info | {result.Findings.Count(finding => finding.Severity == Severity.Info)} |");
        writer.WriteLine($"| Parse errors | {result.ParseErrors.Count} |");
        writer.WriteLine();
    }

    private static void WriteFindings(StringWriter writer, IReadOnlyList<Finding> findings)
    {
        writer.WriteLine("## Findings");
        writer.WriteLine();

        if (findings.Count == 0)
        {
            writer.WriteLine("No cache issues found.");
            writer.WriteLine();
            return;
        }

        writer.WriteLine("| Severity | Rule | Location | Job | Step | Message | Recommendation |");
        writer.WriteLine("| --- | --- | --- | --- | --- | --- | --- |");

        foreach (var finding in findings
            .OrderByDescending(finding => finding.Severity)
            .ThenBy(finding => finding.FilePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(finding => finding.Line ?? int.MaxValue)
            .ThenBy(finding => finding.RuleId, StringComparer.Ordinal))
        {
            writer.WriteLine(
                $"| {Cell(finding.Severity.ToString())} | {Cell(finding.RuleId)} | {Cell(Location(finding.FilePath, finding.Line))} | {Cell(finding.JobId)} | {Cell(finding.StepName)} | {Cell(finding.Message)} | {Cell(finding.Recommendation)} |");
        }

        writer.WriteLine();
    }

    private static void WriteParseErrors(StringWriter writer, IReadOnlyList<WorkflowParseError> parseErrors)
    {
        if (parseErrors.Count == 0)
        {
            return;
        }

        writer.WriteLine("## Parse errors");
        writer.WriteLine();
        writer.WriteLine("| Location | Message |");
        writer.WriteLine("| --- | --- |");

        foreach (var parseError in parseErrors.OrderBy(error => error.FilePath, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine($"| {Cell(Location(parseError.FilePath, parseError.Line))} | {Cell(parseError.Message)} |");
        }

        writer.WriteLine();
    }

    private static string Location(string filePath, int? line) =>
        line is null ? filePath : $"{filePath}:{line}";

    private static string Cell(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        return value
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r\n", "<br>", StringComparison.Ordinal)
            .Replace("\n", "<br>", StringComparison.Ordinal)
            .Replace("\r", "<br>", StringComparison.Ordinal);
    }
}
