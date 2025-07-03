using Associate.Integrations.Activates.GetActivate;
using Associate.Integrations.Activates.GetActivates;
using Associate.Presentation.DTOs;
using MediatR;

namespace Associate.Presentation.GraphQL;

public class AssociatesExperienceQueries(IMediator mediator) : IAssociatesExperienceQueries
{
    public async Task<IReadOnlyCollection<AssociateDto>> GetAllAssociatesAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetActivatesQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve associates information.");
        }
        var associates = result.Value;

        return associates.Select(x => new AssociateDto(
            x.Identification,
            x.DocumentType.ToString(),
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
            associate.DocumentType.ToString(),
            associate.ActivateId,
            associate.Pensioner,
            associate.ActivateDate  
        );
    }
}
