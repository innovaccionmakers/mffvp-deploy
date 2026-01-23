using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reports.Application.LoadingInfo.Audit;

public static class EtlJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static JsonDocument ToJsonDocument<T>(T value)
        => JsonSerializer.SerializeToDocument(value, Options);
}