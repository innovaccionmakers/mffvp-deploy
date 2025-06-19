using Associate.Integrations.Associates;
using Associate.Presentation.DTOs;
using MediatR;

namespace Associate.Presentation.GraphQL;

[ExtendObjectType("Query")]
public class AssociateQueries
{
    public async Task<IReadOnlyCollection<AssociateDto>> GetAssociatesAsync(
        [Service] IMediator mediator,
        string? identificationType,
        string? searchBy,
        string? text,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAssociatesQuery(identificationType, searchBy, text), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve associates.");
        }

        var associates = result.Value;

        return associates.Select(a => new AssociateDto(
            a.IdentificationType,
            a.Identification,
            a.FullName)).ToList();

    }
}

