using System.Text.Json;
using System.Text.Json.Serialization;
using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Reporters;

public sealed class JsonReporter : IReporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string Render(ScanResult result) => JsonSerializer.Serialize(result, Options) + Environment.NewLine;
}
