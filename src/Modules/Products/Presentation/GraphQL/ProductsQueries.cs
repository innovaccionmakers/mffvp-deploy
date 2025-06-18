namespace Products.Presentation.GraphQL;

using MediatR;
using Products.Integrations.Portfolios.Queries;
using Products.Presentation.DTOs;
using Products.Integrations.Banks;


[ExtendObjectType("Query")]
public class ProductsQueries
{
    public async Task<PortfolioDto> GetPortfolioAsync(string objetiveId,
        [Service] IMediator mediator, 
        CancellationToken cancellationToken)
    {

        try
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
        catch (Exception ex)
        {
                // Puedes loguear el error aquí si tienes un sistema de logging
            throw new GraphQLException(ErrorBuilder.New()
            .SetMessage("Ocurrió un error inesperado al obtener el portafolio.")
            .SetCode("UNEXPECTED_ERROR")
            .Build());
        }

    }

    public async Task<IReadOnlyCollection<BankDto>> GetBanksAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
    {

        var result = await mediator.Send(new GetBanksQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve banks.");
        }

        var banks = result.Value;


        return banks.Select(x => new BankDto(
            x.BankId.ToString(),
            x.Name
        )).ToList();

    }
}
