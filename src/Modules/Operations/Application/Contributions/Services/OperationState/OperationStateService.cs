using System;
using System.Text.Json;
using Operations.Application.Abstractions.Services.OperationState;
using Operations.Application.Contributions.CreateContribution;
using Operations.Domain.ConfigurationParameters;

namespace Operations.Application.Contributions.Services.OperationState;

public sealed class OperationStateService(IConfigurationParameterRepository configurationParameterRepository)
    : IOperationStateService
{
    private int? cachedState;

    public async Task<int> GetActiveStateAsync(CancellationToken cancellationToken = default)
    {
        if (cachedState.HasValue)
        {
            return cachedState.Value;
        }

        var parameter = await configurationParameterRepository.GetByUuidAsync(
            ConfigurationParameterUuids.ActiveClientOperationState,
            cancellationToken);

        if (parameter is null)
        {
            throw new InvalidOperationException(
                "El parámetro de configuración para el estado activo de las operaciones no existe.");
        }

        var value = ExtractState(parameter.Metadata);
        cachedState = value;
        return value;
    }

    private static int ExtractState(JsonDocument metadata)
    {
        if (metadata is null)
        {
            throw new InvalidOperationException(
                "El parámetro de configuración para el estado activo no tiene información en metadata.");
        }

        var root = metadata.RootElement;

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("valor", out var valueElement))
        {
            if (TryParseJsonElement(valueElement, out var parsedFromObject))
            {
                return parsedFromObject;
            }
        }

        if (TryParseJsonElement(root, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException(
            "El valor del estado activo de las operaciones no es válido.");
    }

    private static bool TryParseJsonElement(JsonElement element, out int value)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Number when element.TryGetInt32(out var number):
                value = number;
                return true;
            case JsonValueKind.String when int.TryParse(element.GetString(), out var fromString):
                value = fromString;
                return true;
            default:
                value = default;
                return false;
        }
    }
}
