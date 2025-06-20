namespace Operations.Integrations.ConfigurationParameters;

public sealed record ConfigurationParameterResponse(
    string Id,
    string Name,
    string HomologatedCode,
    bool Status
);