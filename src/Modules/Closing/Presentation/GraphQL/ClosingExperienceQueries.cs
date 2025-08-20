using Closing.Integrations.PortfolioValuations.Queries;
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

    public async Task<IReadOnlyCollection<PortfolioValuationDto>> GetPortfolioValuation(DateOnly closingDate, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPortfolioValuationQuery(closingDate.ToDateTime(TimeOnly.MinValue)), cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve results")
                    .Build()
            );
        }

        return result.Value.Select(x => new PortfolioValuationDto(
                x.PortfolioId,
                x.ClosingDate,
                x.Contributions,
                x.Withdrawals,
                x.PygBruto,
                x.Expenses,
                x.CommissionDay,
                x.CostDay,
                x.CreditedYields,
                x.GrossYieldPerUnit,
                x.CostPerUnit,
                x.UnitValue,
                x.Units,
                x.AmountPortfolio,
                x.TrustIds
            )).ToList();
    }
}