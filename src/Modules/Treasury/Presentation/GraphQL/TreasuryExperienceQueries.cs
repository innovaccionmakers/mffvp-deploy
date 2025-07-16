using HotChocolate;
using MediatR;
using Treasury.Integrations.Issuers.GetIssuers;
using Treasury.Presentation.DTOs;

namespace Treasury.Presentation.GraphQL;

public class TreasuryExperienceQueries(IMediator mediator) : ITreasuryExperienceQueries
{
    public async Task<IReadOnlyCollection<IssuerDto>> GetIssuersAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetIssuersQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve Portfolios")
                    .Build()
            );
        }

        var issuers = result.Value;

        return issuers.Select(x => new IssuerDto(
            x.Id,
            x.IssuerCode,
            x.Description,
            x.Nit,
            x.Digit,
            x.HomologatedCode
        )).ToList();
    }
}