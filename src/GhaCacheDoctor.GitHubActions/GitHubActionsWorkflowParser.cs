using GhaCacheDoctor.Core;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace GhaCacheDoctor.GitHubActions;

public sealed class GitHubActionsWorkflowParser : IWorkflowParser
{
    public WorkflowParseResult Parse(string filePath)
    {
        try
        {
            using var reader = File.OpenText(filePath);
            var yaml = new YamlStream();
            yaml.Load(reader);

            if (yaml.Documents.Count == 0 || yaml.Documents[0].RootNode is not YamlMappingNode root)
            {
                return new WorkflowParseResult(new WorkflowDocument(filePath, null, []), null);
            }

            var name = GetScalar(root, "name");
            var jobs = ParseJobs(root);
            return new WorkflowParseResult(new WorkflowDocument(filePath, name, jobs), null);
        }
        catch (YamlException ex)
        {
            int? line = ex.Start.Line > 0 ? (int)ex.Start.Line : null;
            return new WorkflowParseResult(null, new WorkflowParseError(filePath, line, ex.Message));
        }
        catch (IOException ex)
        {
            return new WorkflowParseResult(null, new WorkflowParseError(filePath, null, ex.Message));
        }
    }

    private static IReadOnlyList<WorkflowJob> ParseJobs(YamlMappingNode root)
    {
        if (!TryGetMapping(root, "jobs", out var jobsNode))
        {
            return [];
        }

        var jobs = new List<WorkflowJob>();
        foreach (var (key, value) in jobsNode.Children)
        {
            if (key is not YamlScalarNode jobIdNode || value is not YamlMappingNode jobNode || jobIdNode.Value is null)
            {
                continue;
            }

            jobs.Add(new WorkflowJob(jobIdNode.Value, GetScalar(jobNode, "name"), ParseSteps(jobNode)));
        }

        return jobs;
    }

    private static IReadOnlyList<WorkflowStep> ParseSteps(YamlMappingNode jobNode)
    {
        if (!TryGetSequence(jobNode, "steps", out var stepsNode))
        {
            return [];
        }

        var steps = new List<WorkflowStep>();
        foreach (var stepNode in stepsNode.Children.OfType<YamlMappingNode>())
        {
            steps.Add(new WorkflowStep(
                GetScalar(stepNode, "name"),
                GetScalar(stepNode, "uses"),
                GetScalar(stepNode, "run"),
                GetWithValues(stepNode),
                stepNode.Start.Line > 0 ? (int)stepNode.Start.Line : null));
        }

        return steps;
    }

    private static IReadOnlyDictionary<string, string> GetWithValues(YamlMappingNode stepNode)
    {
        if (!TryGetMapping(stepNode, "with", out var withNode))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in withNode.Children)
        {
            if (key is YamlScalarNode keyNode && keyNode.Value is not null)
            {
                values[keyNode.Value] = NodeToString(value);
            }
        }

        return values;
    }

    private static string? GetScalar(YamlMappingNode node, string key)
    {
        return TryGetNode(node, key, out var value) ? NodeToString(value) : null;
    }

    private static bool TryGetMapping(YamlMappingNode node, string key, out YamlMappingNode mapping)
    {
        if (TryGetNode(node, key, out var value) && value is YamlMappingNode mappingNode)
        {
            mapping = mappingNode;
            return true;
        }

        mapping = null!;
        return false;
    }

    private static bool TryGetSequence(YamlMappingNode node, string key, out YamlSequenceNode sequence)
    {
        if (TryGetNode(node, key, out var value) && value is YamlSequenceNode sequenceNode)
        {
            sequence = sequenceNode;
            return true;
        }

        sequence = null!;
        return false;
    }

    private static bool TryGetNode(YamlMappingNode node, string key, out YamlNode value)
    {
        foreach (var (childKey, childValue) in node.Children)
        {
            if (childKey is YamlScalarNode scalar && scalar.Value?.Equals(key, StringComparison.OrdinalIgnoreCase) == true)
            {
                value = childValue;
                return true;
            }
        }

        value = null!;
        return false;
    }

    private static string NodeToString(YamlNode node)
    {
        return node switch
        {
            YamlScalarNode scalar => scalar.Value ?? string.Empty,
            YamlSequenceNode sequence => string.Join(Environment.NewLine, sequence.Children.Select(NodeToString)),
            _ => node.ToString()
        };
    }
}
