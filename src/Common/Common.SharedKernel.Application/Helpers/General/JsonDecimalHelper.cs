
using System.Globalization;
using System.Text.Json;

namespace Common.SharedKernel.Application.Helpers.General;

/// <summary>
/// Helper para extraer valores decimales desde JsonDocument.
/// Soporta números, cadenas con coma/punto y porcentajes.
/// </summary>
public static class JsonDecimalHelper
{
    /// <summary>
    /// Extrae un valor decimal de una propiedad específica en un JsonDocument.
    /// Soporta valores numéricos o cadenas con separadores decimales y porcentaje.
    /// </summary>
    /// <param name="json">JsonDocument a procesar.</param>
    /// <param name="propertyName">Nombre de la propiedad a buscar.</param>
    /// <param name="isPercentage">Indica si el valor es porcentaje (divide entre 100).</param>
    /// <returns>Valor decimal convertido, o 0 si no se puede parsear.</returns>
    public static decimal ExtractDecimal(JsonDocument json, string propertyName, bool isPercentage = false)
    {
        if (json is null) return 0m;

        var root = json.RootElement;

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(propertyName, out var valueElement))
        {
            decimal result = 0m;

            if (valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetDecimal(out var number))
            {
                result = number;
            }
            else if (valueElement.ValueKind == JsonValueKind.String)
            {
                result = ParseDecimal(valueElement.GetString());
            }

            return isPercentage ? result / 100m : result;
        }

        return 0m;
    }

    /// <summary>
    /// Extrae múltiples valores decimales de un JsonDocument en base a una lista de propiedades.
    /// </summary>
    /// <param name="json">JsonDocument a procesar.</param>
    /// <param name="properties">Diccionario con nombre de propiedad y flag isPercentage.</param>
    /// <returns>Diccionario con los valores extraídos.</returns>
    public static Dictionary<string, decimal> ExtractMultiple(JsonDocument json, Dictionary<string, bool> properties)
    {
        var result = new Dictionary<string, decimal>();

        if (json is null || properties == null) return result;

        foreach (var kvp in properties)
        {
            var propertyName = kvp.Key;
            var isPercentage = kvp.Value;
            var value = ExtractDecimal(json, propertyName, isPercentage);
            result[propertyName] = value;
        }

        return result;
    }

    /// <summary>
    /// Convierte un string a decimal, soportando formatos con coma, punto y símbolo de porcentaje.
    /// </summary>
    private static decimal ParseDecimal(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0m;

        var sanitized = raw.Replace("%", "", StringComparison.Ordinal).Trim()
                           .Replace(',', '.');

        return decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out var val)
            ? val
            : 0m;
    }
}
