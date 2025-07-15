using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Products.Commissions;

public interface ICommissionLocator
{
    Task<Result<IReadOnlyCollection<CommissionsByPortfolioRemoteResponse>>> GetActiveCommissionsAsync(
         int portfolioId,
         CancellationToken cancellationToken);
}
public sealed record CommissionsByPortfolioRemoteResponse(
    int CommissionId,
    string Concept,
    string Modality,
    string CommissionType,
    string Period,
    string CalculationBase,
    string CalculationRule
    );
