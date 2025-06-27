namespace Products.Integrations.ConfigurationParameters.DocumentTypes;

public sealed record ConfigurationParameterResponse(
    string Id,
    Guid Uuid,
    string Name,
    string HomologatedCode,
    bool Status
);