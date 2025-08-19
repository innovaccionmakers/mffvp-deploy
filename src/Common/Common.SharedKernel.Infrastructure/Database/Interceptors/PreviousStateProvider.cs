using System.Text.Json;
using Common.SharedKernel.Application.Abstractions;

namespace Common.SharedKernel.Infrastructure.Database.Interceptors;

public sealed class PreviousStateProvider : IPreviousStateProvider
{
    private readonly List<object> _states = new();

    public void AddState(string entityName, IDictionary<string, object?> values)
    {
        _states.Add(new { Entity = entityName, Values = values });
    }

    public JsonDocument GetSerializedStateAndClear()
    {
        var json = _states.Count == 0
            ? JsonDocument.Parse("{}")
            : JsonSerializer.SerializeToDocument(_states);
        _states.Clear();
        return json;
    }
}