namespace Treasury.IntegrationEvents.Issuers.GetIssuersByIds;

public sealed record IssuerResponse(
    long Id,
    string IssuerCode,
    string Description,
    string Nit,
    int Digit,
    string HomologatedCode,
    bool IsBank
);

