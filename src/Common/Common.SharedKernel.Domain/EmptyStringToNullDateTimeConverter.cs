using System.Text.Json;
using System.Text.Json.Serialization;
namespace Common.SharedKernel.Domain
{
    public class EmptyStringToNullDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrEmpty(stringValue))
                        return null;

                    // Parsear la fecha y asegurar que es UTC
                    var date = DateTime.Parse(stringValue);
                    return DateTime.SpecifyKind(date, DateTimeKind.Utc);
                case JsonTokenType.Null:
                case JsonTokenType.True:
                case JsonTokenType.False:
                case JsonTokenType.Number:
                    return null;
                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString("yyyy-MM-dd"));
        }
    }
}