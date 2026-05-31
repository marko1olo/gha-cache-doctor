namespace GhaCacheDoctor.Core;

public interface IRule
{
    string Id { get; }
    string Title { get; }
    Severity DefaultSeverity { get; }
    string Category { get; }

    IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false);
}

public interface IWorkflowParser
{
    WorkflowParseResult Parse(string filePath);
}
