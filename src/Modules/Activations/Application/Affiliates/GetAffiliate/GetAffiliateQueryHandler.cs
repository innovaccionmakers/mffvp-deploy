using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Activations.Domain.Affiliates;
using Activations.Integrations.Affiliates.GetAffiliate;
using Activations.Integrations.Affiliates;

namespace Activations.Application.Affiliates.GetAffiliate;

internal sealed class GetAffiliateQueryHandler(
    IAffiliateRepository affiliateRepository)
    : IQueryHandler<GetAffiliateQuery, AffiliateResponse>
{
    public async Task<Result<AffiliateResponse>> Handle(GetAffiliateQuery request, CancellationToken cancellationToken)
    {
        var affiliate = await affiliateRepository.GetAsync(request.AffiliateId, cancellationToken);
        if (affiliate is null)
        {
            return Result.Failure<AffiliateResponse>(AffiliateErrors.NotFound(request.AffiliateId));
        }
        var response = new AffiliateResponse(
            affiliate.AffiliateId,
            affiliate.IdentificationType,
            affiliate.Identification,
            affiliate.Pensioner,
            affiliate.MeetsRequirements,
            affiliate.ActivationDate
        );
        return response;
    }
}