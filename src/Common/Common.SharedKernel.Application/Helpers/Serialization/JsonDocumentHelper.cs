using System.Text.Json;

namespace Common.SharedKernel.Application.Helpers.Serialization;

public static class JsonDocumentHelper
{
    public static string NormalizeJson(JsonDocument jsonDocument)
    {
        if (jsonDocument == null)
            return string.Empty;

        var options = new JsonSerializerOptions
        {
            WriteIndented = false
        };
        return JsonSerializer.Serialize(jsonDocument.RootElement, options);
    }
}

