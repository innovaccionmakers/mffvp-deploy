namespace Products.Presentation.GraphQL;

using MediatR;
using Products.Integrations.Portfolios.Queries;
using Products.Presentation.DTOs;


[ExtendObjectType("Query")]
public class ProductsQueries
{
    public async Task<PortfolioDto> GetPortfolioAsync(string objetiveId,
        [Service] IMediator mediator, 
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPortfolioInformationByObjetiveIdQuery(objetiveId), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve portfolio information.");
        }

        var portfolioInformation = result.Value;


        return new PortfolioDto(
           portfolioInformation.Found,
           portfolioInformation.Plan,
           portfolioInformation.Alternative,
           portfolioInformation.Portfolio);     

    }
}
