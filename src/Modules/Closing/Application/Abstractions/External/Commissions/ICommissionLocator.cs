
using Common.SharedKernel.Domain;
//using Products.Integrations.Commissions.Response;

namespace Closing.Application.Abstractions.External.Commissions;

public interface ICommissionLocator
{
    Task<Result<IReadOnlyCollection<GetCommissionsByPortfolioIdResponse>>> GetActiveCommissionsAsync(
         int portfolioId,
         CancellationToken cancellationToken);
}
public sealed record CommissionsByPortfolioResponse
(bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<GetCommissionsByPortfolioIdResponse> Commissions
);

public sealed record GetCommissionsByPortfolioIdResponse(
    int CommissionId,
    string Concept,
    string Modality,
    string CommissionType,
    string Period,
    string CalculationBase,
    string CalculationRule
    );
public sealed record CommissionsByPortfolioRequest
(int PortfolioId
);