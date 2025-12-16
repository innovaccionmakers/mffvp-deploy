using System.Text.Json;
using System.Text.Json.Serialization;

namespace Security.Application.Contracts.Permissions;

public class PermissionDtoJsonConverter : JsonConverter<PermissionDtoBase>
{
    public override PermissionDtoBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("subResource", out _))
        {
            return JsonSerializer.Deserialize<PermissionWithSubResourceDto>(root.GetRawText(), options)!;
        }

        return JsonSerializer.Deserialize<PermissionDto>(root.GetRawText(), options)!;
    }

    public override void Write(Utf8JsonWriter writer, PermissionDtoBase value, JsonSerializerOptions options)
    {
        var type = value.GetType();

        var optionsWithoutConverter = new JsonSerializerOptions(options);
        optionsWithoutConverter.Converters.Remove(this);

        JsonSerializer.Serialize(writer, value, type, optionsWithoutConverter);
    }
}

