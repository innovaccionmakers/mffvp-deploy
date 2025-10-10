using Common.SharedKernel.Domain;
namespace Products.Application.Abstractions.External.Closing;

public interface IPortfolioValuationLocator
{
    Task<Result<IReadOnlyCollection<PortfolioValuationResponse>>> GetPortfolioValuationAsync(DateTime closingDate, CancellationToken cancellationToken = default);
}


public sealed record PortfolioValuationResponse(
    int PortfolioId,
    DateTime ClosingDate,
    decimal Contributions,
    decimal Withdrawals,
    decimal PygBruto,
    decimal Expenses,
    decimal CommissionDay,
    decimal CostDay,
    decimal YieldToCredit,
    decimal GrossYieldPerUnit,
    decimal CostPerUnit,
    decimal UnitValue,
    decimal Units,
    decimal AmountPortfolio,
    IEnumerable<long> TrustIds
);