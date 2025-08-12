using MediatR;
using Trusts.Integrations.Trusts.Queries;

namespace Trusts.Presentation.GraphQL;

public class TrustExperienceQueries(IMediator mediator) : ITrustExperienceQueries
{
    public async Task<int> GetParticipantAsync(IEnumerable<long> trustIds, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetParticipantQuery(trustIds), cancellationToken);

        if(!result.IsSuccess)
                return 0;

        return result.Value;
    }
}
