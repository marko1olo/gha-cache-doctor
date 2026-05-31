using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Reporters;

public interface IReporter
{
    string Render(ScanResult result);
}
