namespace GhaCacheDoctor.Core;

public interface IRule
{
    string Id { get; }
    string Title { get; }
    Severity DefaultSeverity { get; }
    string Category { get; }

    IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository);
}

public interface IWorkflowParser
{
    WorkflowParseResult Parse(string filePath);
}
