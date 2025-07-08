using Common.SharedKernel.Application.Messaging;
using Products.Domain.Commissions;
using Products.Integrations.Commissions.Queries;
using Common.SharedKernel.Domain;
using Products.Integrations.Commissions.Response;

namespace Products.Application.Commissions.Queries;

internal sealed class GetCommissionsByPortfolioIdQueryHandler(
ICommissionRepository commissionRepository) : IQueryHandler<GetCommissionsByPortfolioIdQuery, IReadOnlyCollection<GetCommissionsByPortfolioIdResponse>>
{
    public async Task<Result<IReadOnlyCollection<GetCommissionsByPortfolioIdResponse>>> Handle(GetCommissionsByPortfolioIdQuery request,
      CancellationToken cancellationToken)
    {
        var activeCommissions = await commissionRepository.GetActiveCommissionsByPortfolioAsync(request.PortfolioId, cancellationToken);


        var response = activeCommissions
             .Select(c => new GetCommissionsByPortfolioIdResponse(
                 c.CommissionId, 
                 c.Concept, 
                 c.Modality, 
                 c.CommissionType,
                 c.Period, 
                 c.CalculationBase, 
                 c.CalculationRule
               ))
             .ToList();

        return response;
    }
}
