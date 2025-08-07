
using Azure;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;

namespace Products.Application.Portfolios.QueriesHandlers;

internal sealed class GetPortfoliosByIdsQueryHandler(IPortfolioRepository repository) : IQueryHandler<GetPortfoliosByIdsQuery, IReadOnlyCollection<PortfolioResponse>>
{
    public async Task<Result<IReadOnlyCollection<PortfolioResponse>>> Handle(GetPortfoliosByIdsQuery request, CancellationToken cancellationToken)
    {
        var portfolios = await repository.GetPortfoliosByIdsAsync(request.PortfolioIds, cancellationToken);
        var result = portfolios.Select(portfolio => new PortfolioResponse(
            portfolio.PortfolioId,
            portfolio.HomologatedCode,
            portfolio.Name,
            portfolio.ShortName,
            portfolio.ModalityId,
            portfolio.InitialMinimumAmount,
            portfolio.CurrentDate
        )).ToList();

        return Result.Success<IReadOnlyCollection<PortfolioResponse>>(result);
    }
}
