namespace Treasury.Presentation.DTOs;

public record BankAccountDto(
    long Id,
    long PortfolioId,
    long IssuerId,
    string IssuerName,
    string IssuerDescription,
    string AccountNumber,
    string AccountType, // "Corriente" o "Ahorros"
    string Observations,
    DateTime ProcessDate
);