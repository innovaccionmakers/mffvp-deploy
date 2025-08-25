
using Closing.Integrations.Common;

namespace Closing.Application.Closing.Services.Warnings;

public interface IWarningCollector
{
    void Add(WarningItem warning);
    void AddRange(IEnumerable<WarningItem> warnings);
    IReadOnlyList<WarningItem> GetAll();
    bool HasAny();
}