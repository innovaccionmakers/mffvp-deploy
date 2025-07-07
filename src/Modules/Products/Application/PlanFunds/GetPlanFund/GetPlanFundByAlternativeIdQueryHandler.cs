using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.PlanFunds;
using Products.Integrations.PlanFunds.GetPlanFund;

namespace Products.Application.PlanFunds.GetPlanFund;

internal sealed class GetPlanFundByAlternativeIdQueryHandler(
    IPlanFundRepository repository)
    : IQueryHandler<GetPlanFundByAlternativeIdQuery, PlanFundQueryResponse>
{
    public async Task<Result<PlanFundQueryResponse>> Handle(
        GetPlanFundByAlternativeIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPlanFundByAlternativeIdAsync(request.AlternativeId, cancellationToken);
        return Result.Success(result);
    }
}
