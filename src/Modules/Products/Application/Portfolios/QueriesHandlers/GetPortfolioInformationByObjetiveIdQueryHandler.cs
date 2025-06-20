using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;
using System.Text.Json;


namespace Products.Application.Portfolios.QueriesHandlers;

internal sealed class GetPortfolioInformationByObjetiveIdQueryHandler(
    IPortfolioRepository repository)
    : IQueryHandler<GetPortfolioInformationByObjetiveIdQuery, PortfolioInformation>
{
    public async Task<Result<PortfolioInformation>> Handle(
    GetPortfolioInformationByObjetiveIdQuery request,
    CancellationToken cancellationToken)
    {
        var result = await repository.GetPortfolioInformationByObjectiveIdAsync(request.ObjetiveId, cancellationToken);

        return Result.Success(result);
    }

}