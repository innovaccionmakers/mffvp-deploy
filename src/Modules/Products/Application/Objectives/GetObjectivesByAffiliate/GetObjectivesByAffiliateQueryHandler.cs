using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Objectives;
using Products.Integrations.Objectives.GetObjectivesByAffiliate;

namespace Products.Application.Objectives.GetObjectivesByAffiliate;

internal sealed class GetObjectivesByAffiliateQueryHandler(
    IObjectiveRepository objectiveRepository
) : IQueryHandler<GetObjectivesByAffiliateQuery, IReadOnlyCollection<AffiliateObjectiveQueryResponse>>
{
    public async Task<Result<IReadOnlyCollection<AffiliateObjectiveQueryResponse>>> Handle(
        GetObjectivesByAffiliateQuery request,
        CancellationToken cancellationToken)
    {
        var result = await objectiveRepository.GetAffiliateObjectivesByAffiliateIdAsync(request.AffiliateId, cancellationToken);

        return Result.Success(result);
    }
}