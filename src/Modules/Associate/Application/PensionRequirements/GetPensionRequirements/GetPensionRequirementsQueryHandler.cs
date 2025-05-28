using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements.GetPensionRequirements;
using Associate.Integrations.PensionRequirements;

namespace Associate.Application.PensionRequirements.GetPensionRequirements;

internal sealed class GetPensionRequirementsQueryHandler(
    IPensionRequirementRepository pensionrequirementRepository)
    : IQueryHandler<GetPensionRequirementsQuery, IReadOnlyCollection<PensionRequirementResponse>>
{
    public async Task<Result<IReadOnlyCollection<PensionRequirementResponse>>> Handle(GetPensionRequirementsQuery request, CancellationToken cancellationToken)
    {
        var entities = await pensionrequirementRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new PensionRequirementResponse(
                e.PensionRequirementId,
                e.AffiliateId,
                e.StartDate,
                e.ExpirationDate,
                e.CreationDate,
                e.Status))
            .ToList();

        return Result.Success<IReadOnlyCollection<PensionRequirementResponse>>(response);
    }
}