using Closing.Integrations.ProfitLosses.GetProfitandLoss;
using Closing.Presentation.GraphQL.DTOs;
using HotChocolate;
using MediatR;

namespace Closing.Presentation.GraphQL;

public class ClosingExperienceQueries(IMediator mediator) : IClosingExperienceQueries
{
    public async Task<ProfitAndLossDto?> GetProfitAndLossAsync(int portfolioId, DateTime effectiveDate, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetProfitandLossQuery(portfolioId, effectiveDate), cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve results")
                    .Build()
            );
        }

        return new ProfitAndLossDto(
            result.Value.Values,
            result.Value.NetYield
        );
    }
}