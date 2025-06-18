using System.Text.Json;
using System.Text.Json.Serialization;
namespace Common.SharedKernel.Domain
{
    public class EmptyStringToNullDateTimeConverter: JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            return string.IsNullOrEmpty(stringValue) ? null : DateTime.Parse(stringValue);
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString("yyyy-MM-dd"));
    }
}
}