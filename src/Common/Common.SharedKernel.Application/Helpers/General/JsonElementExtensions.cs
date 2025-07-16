using System.Text.Json;

public static class JsonElementExtensions
{
    public static Dictionary<string, decimal> ToDecimalDictionary(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("El JsonElement debe ser un objeto JSON.");
        }

        var result = new Dictionary<string, decimal>();

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Number ||
                !property.Value.TryGetDecimal(out var value))
            {
                throw new InvalidOperationException($"El valor de '{property.Name}' no es un número decimal válido.");
            }

            result[property.Name] = value;
        }

        return result;
    }
}