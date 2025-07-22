
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using System.Text.Json;

namespace Common.SharedKernel.Application.Caching.Closing;

public class JsonClosingExecutionSerializer : IClosingExecutionSerializer
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public ClosingExecutionState? Deserialize(string json)
        => JsonSerializer.Deserialize<ClosingExecutionState>(json, _options);

    public string Serialize(ClosingExecutionState state)
        => JsonSerializer.Serialize(state, _options);
}