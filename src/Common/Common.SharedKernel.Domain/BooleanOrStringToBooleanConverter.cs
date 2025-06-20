using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.SharedKernel.Domain
{
    public class BooleanOrStringToBooleanConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.String:
                    return string.IsNullOrEmpty(reader.GetString()) ? null : bool.Parse(reader.GetString());
                case JsonTokenType.Null:
                case JsonTokenType.Number:
                    return null;
                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteBooleanValue(value.Value);
            else
                writer.WriteNullValue();
        }    
    }
}