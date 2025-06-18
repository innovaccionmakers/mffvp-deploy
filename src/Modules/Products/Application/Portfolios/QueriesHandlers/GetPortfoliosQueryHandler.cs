using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;

namespace Products.Application.Portfolios.QueriesHandlers;

internal sealed class GetPortfolioInformationByObjetiveIdQueryHandler(
    IPortfolioRepository repository)
    : IQueryHandler<GetPortfolioInformationByObjetiveIdQuery, IReadOnlyCollection<PortfolioInformationResponse>>
{
    public async Task<Result<IReadOnlyCollection<PortfolioInformationResponse>>> Handle(
        string ObjetiveId,
        GetPortfolioInformationByObjetiveIdQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(p => new PortfolioInformationResponse(
                p.Name,
                p.Name,
                p.Name,
                p.Name))
            .ToList();

        return Result.Success<IReadOnlyCollection<PortfolioInformationResponse>>(response);
    }
}