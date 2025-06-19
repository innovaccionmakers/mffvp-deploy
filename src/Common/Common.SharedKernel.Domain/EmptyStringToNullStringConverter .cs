using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.SharedKernel.Domain
{
    public class EmptyStringToNullStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    return string.IsNullOrEmpty(stringValue) ? null : stringValue;
                case JsonTokenType.Null:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                    return null;
                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value ?? string.Empty);
        }
    }
}