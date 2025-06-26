namespace Operations.Integrations.ConfigurationParameters;

public sealed record ConfigurationParameterResponse(
    string Id,
    Guid Uuid,
    string Name,
    string HomologatedCode,
    bool Status
);