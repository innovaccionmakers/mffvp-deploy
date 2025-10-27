namespace Operations.Integrations.ConfigurationParameters;

public sealed record CancellationClauseConfigurationParameterResponse(
    string ConfigurationParameterId,
    Guid Uuid,
    string Name,
    string HomologationCode,
    bool Status
);
