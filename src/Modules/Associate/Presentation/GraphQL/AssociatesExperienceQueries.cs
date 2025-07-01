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
            x.Pensioner
        )).ToList();
    }
}
