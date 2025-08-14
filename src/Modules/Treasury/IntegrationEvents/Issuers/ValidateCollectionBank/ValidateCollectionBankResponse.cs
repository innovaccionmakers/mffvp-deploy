namespace Treasury.IntegrationEvents.Issuers.ValidateCollectionBank;

public sealed record ValidateCollectionBankResponse(
    bool IsValid,
    string? Code,
    string? Message,
    long? BankId);