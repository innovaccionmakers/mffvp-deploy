using System.Globalization;
using System.Text.Json;

namespace Common.SharedKernel.Application.Helpers.General;

/// <summary>
/// Helper para extraer valores enteros Int32 desde JsonDocument.
/// Soporta valores numéricos nativos y cadenas con separadores simples.
/// </summary>
public static class JsonIntegerHelper
{
    /// <summary>
    /// Extrae un Int32 de una propiedad específica en un JsonDocument.
    /// Si no existe o no se puede parsear, retorna <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="json">JsonDocument a procesar.</param>
    /// <param name="propertyName">Nombre de la propiedad a buscar.</param>
    /// <param name="defaultValue">Valor por defecto si no se puede convertir.</param>
    /// <returns>Entero convertido.</returns>
    public static int ExtractInt32(JsonDocument json, string propertyName, int defaultValue = 0)
    {
        if (json is null) return defaultValue;

        var root = json.RootElement;

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(propertyName, out var valueElement))
        {
            if (valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetInt32(out var number))
            {
                return number;
            }
            else if (valueElement.ValueKind == JsonValueKind.String)
            {
                return ParseInt32(valueElement.GetString(), defaultValue);
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Extrae múltiples Int32 de un JsonDocument en base a una lista de propiedades.
    /// </summary>
    public static Dictionary<string, int> ExtractMultiple(JsonDocument json, IEnumerable<string> properties, int defaultValue = 0)
    {
        var result = new Dictionary<string, int>();

        if (json is null || properties is null) return result;

        foreach (var property in properties)
        {
            result[property] = ExtractInt32(json, property, defaultValue);
        }

        return result;
    }

    /// <summary>
    /// Convierte un string a Int32, soportando separadores de miles simples (coma, punto, espacio).
    /// </summary>
    private static int ParseInt32(string? raw, int defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(raw)) return defaultValue;

        var sanitized = raw.Trim()
                           .Replace(",", "")
                           .Replace(".", "")
                           .Replace(" ", "");

        return int.TryParse(sanitized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val)
            ? val
            : defaultValue;
    }
}

