
using Closing.Integrations.Common;
using System.Collections.Concurrent;

namespace Closing.Application.Closing.Services.Warnings;

public sealed class WarningCollector : IWarningCollector
{
    private readonly ConcurrentBag<WarningItem> _items = new(); //ConcurrentBag para manejar acceso concurrente en varios hilos

    public void Add(WarningItem warning)
    {
        if (warning is not null)
            _items.Add(warning);
    }

    public void AddRange(IEnumerable<WarningItem> warnings)
    {
        if (warnings is null) return;
        foreach (var w in warnings)
            if (w is not null) _items.Add(w);
    }

    public IReadOnlyList<WarningItem> GetAll() => _items.ToArray();

    public bool HasAny() => !_items.IsEmpty;
}
