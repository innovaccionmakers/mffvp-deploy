using System.Text.Json;

namespace Common.SharedKernel.Application.Helpers.Serialization;

/// <summary>
/// Helper para extraer valores de tipo string desde JsonDocument.
/// Soporta valores string nativos y convierte números u otros tipos a string cuando es posible.
/// </summary>
public static class JsonStringHelper
{
    /// <summary>
    /// Extrae un string de una propiedad específica en un JsonDocument.
    /// Si no existe o no se puede obtener, retorna <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="json">JsonDocument a procesar.</param>
    /// <param name="propertyName">Nombre de la propiedad a buscar.</param>
    /// <param name="defaultValue">Valor por defecto si no se puede convertir.</param>
    /// <returns>Cadena convertida.</returns>
    public static string ExtractString(JsonDocument json, string propertyName, string defaultValue = "")
    {
        if (json is null) return defaultValue;

        var root = json.RootElement;

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(propertyName, out var valueElement))
        {
            if (valueElement.ValueKind == JsonValueKind.String)
            {
                return valueElement.GetString() ?? defaultValue;
            }
            else if (valueElement.ValueKind == JsonValueKind.Number ||
                     valueElement.ValueKind == JsonValueKind.True ||
                     valueElement.ValueKind == JsonValueKind.False)
            {
                return valueElement.ToString();
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Extrae múltiples strings de un JsonDocument en base a una lista de propiedades.
    /// </summary>
    /// <param name="json">JsonDocument a procesar.</param>
    /// <param name="properties">Lista de propiedades a extraer.</param>
    /// <param name="defaultValue">Valor por defecto por propiedad si no se puede obtener.</param>
    /// <returns>Diccionario propiedad → valor extraído.</returns>
    public static Dictionary<string, string> ExtractMultiple(JsonDocument json, IEnumerable<string> properties, string defaultValue = "")
    {
        var result = new Dictionary<string, string>();
        if (json is null || properties is null) return result;

        foreach (var property in properties)
        {
            result[property] = ExtractString(json, property, defaultValue);
        }

        return result;
    }
}
