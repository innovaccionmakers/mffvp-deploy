using Associate.Integrations.Activates.GetActivate;
using Associate.Integrations.Activates.GetActivates;
using Associate.Integrations.PensionRequirements.GetPensionRequirements;
using Associate.Presentation.DTOs;
using MediatR;

using Microsoft.AspNetCore.Authorization;

namespace Associate.Presentation.GraphQL;

public class AssociatesExperienceQueries(IMediator mediator) : IAssociatesExperienceQueries
{
    //[HotChocolate.Authorization.Authorize(Policy = "fvp:associate:activates:view")]
    public async Task<IReadOnlyCollection<AssociateDto>> GetAllAssociatesAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetActivatesQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null) return [];

        var associates = result.Value;

        return associates.Select(x => new AssociateDto(
            x.Identification,
            x.DocumentType,
            x.DocumentTypeUuid,
            x.ActivateId,
            x.Pensioner,
            x.ActivateDate
        )).ToList();
    }

    public async Task<AssociateDto?> GetAssociateByIdAsync(long associateId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetActivateQuery(associateId), cancellationToken);

        if (!result.IsSuccess) return null;

        var associate = result.Value;

        return new AssociateDto(
            associate.Identification,
            associate.DocumentType,
            associate.DocumentTypeUuid,
            associate.ActivateId,
            associate.Pensioner,
            associate.ActivateDate  
        );
    }

    public async Task<IReadOnlyCollection<PensionRequirementDto>> GetPensionRequirementsByAssociateAsync(int associateId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetPensionRequirementsByAssociateQuery(associateId), cancellationToken);

        if (!result.IsSuccess || result.Value == null) return [];

        var pensionRequirements = result.Value;

        return pensionRequirements.Select(pr => new PensionRequirementDto(
            pr.PensionRequirementId,
            pr.StartDate,
            pr.ExpirationDate,
            pr.Status
        )).ToList();
    }
}
