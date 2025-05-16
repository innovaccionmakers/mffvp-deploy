using System.Text.Json;

namespace Products.Integrations.ConfigurationParameters;

public sealed record ConfigurationParameterResponse(
    int    ConfigurationParameterId,
    Guid   Uuid,
    string Name,
    int?   ParentId,
    bool   Status,
    string Type,
    bool   Editable,
    bool   System,
    JsonDocument Metadata,
    string HomologationCode
);