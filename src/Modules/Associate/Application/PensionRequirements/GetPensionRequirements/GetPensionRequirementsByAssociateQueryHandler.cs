using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements;
using Associate.Integrations.PensionRequirements.GetPensionRequirements;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Application.PensionRequirements.GetPensionRequirements;

internal sealed class GetPensionRequirementsByAssociateQueryHandler(IPensionRequirementRepository repository) : IQueryHandler<GetPensionRequirementsByAssociateQuery, IReadOnlyCollection<PensionRequirementResponse>>
{
    public async Task<Result<IReadOnlyCollection<PensionRequirementResponse>>> Handle(GetPensionRequirementsByAssociateQuery request, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllByAssociateAsync(request.AssociateId, cancellationToken);

        var response = entities
            .Select(e => new PensionRequirementResponse(
                e.PensionRequirementId,
                e.ActivateId,
                e.StartDate,
                e.ExpirationDate,
                e.CreationDate,
                e.Status))
            .ToList();

        return Result.Success<IReadOnlyCollection<PensionRequirementResponse>>(response);
    }
}
