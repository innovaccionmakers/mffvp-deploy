namespace Treasury.Integrations.ConfigurationParameters.AccountTypes;

public sealed record AccountTypeResponse(
    int ConfigurationParameterId,
    Guid Uuid,
    string Name,
    string HomologationCode
);

