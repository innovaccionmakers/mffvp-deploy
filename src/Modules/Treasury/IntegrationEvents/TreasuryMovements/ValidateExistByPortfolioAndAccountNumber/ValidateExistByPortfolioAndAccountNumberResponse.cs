namespace Treasury.IntegrationEvents.TreasuryMovements.ValidateExistByPortfolioAndAccountNumber;

public sealed record ValidateExistByPortfolioAndAccountNumberResponse(
    bool Succeeded,
    string? Code,
    string? Message,
    bool Exist);
