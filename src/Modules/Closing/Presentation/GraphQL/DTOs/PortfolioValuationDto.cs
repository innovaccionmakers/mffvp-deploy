namespace Closing.Presentation.GraphQL.DTOs;

public sealed record PortfolioValuationDto(
    int PortfolioId, 
    DateTime ClosingDate,
    decimal Contributions,
    decimal Withdrawals, 
    decimal PygBruto, 
    decimal Expenses,
    decimal CommissionDay, 
    decimal CostDay, 
    decimal CreditedYields,
    decimal GrossYieldPerUnit,
    decimal CostPerUnit,
    decimal UnitValue,
    decimal Units,
    decimal AmountPortfolio,
    IEnumerable<long> TrustIds
);
