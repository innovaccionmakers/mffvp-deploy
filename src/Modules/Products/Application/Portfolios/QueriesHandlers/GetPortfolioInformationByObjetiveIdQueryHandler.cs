using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;
using System.Text.Json;


namespace Products.Application.Portfolios.QueriesHandlers;

internal sealed class GetPortfolioInformationByObjetiveIdQueryHandler(
    IPortfolioRepository repository)
    : IQueryHandler<GetPortfolioInformationByObjetiveIdQuery, PortfolioInformationResponse>
{
    public async Task<Result<PortfolioInformationResponse>> Handle(
    GetPortfolioInformationByObjetiveIdQuery request,
    CancellationToken cancellationToken)
    {
        var result = await repository.GetPortfolioInformationByObjectiveIdAsync(request.ObjetiveId, cancellationToken);

        if (result is null)
        {
            return Result.Failure<PortfolioInformationResponse>("No se encontró información para el objetivo.");
        }

        return Result.Success(result);
    }

}