namespace Products.Integrations.ConfigurationParameters.DocumentTypes;

public sealed record ConfigurationParameterResponse(
    string Id,
    string Name,
    string HomologatedCode,
    bool Status
);