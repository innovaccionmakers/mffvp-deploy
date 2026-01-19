using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios.Queries;

namespace Products.Application.Portfolios.QueriesHandlers;

internal sealed class GetPortfoliosBasicInformationByIdsQueryHandler(IPortfolioRepository repository)
    : IQueryHandler<GetPortfoliosBasicInformationByIdsQuery, IReadOnlyCollection<PortfolioBasicInformationResponse>>
{
    public async Task<Result<IReadOnlyCollection<PortfolioBasicInformationResponse>>> Handle(
        GetPortfoliosBasicInformationByIdsQuery request,
        CancellationToken cancellationToken)
    {
        var portfolios = await repository.GetPortfoliosByIdsAsync(
            request.PortfolioIds.Select(id => (long)id),
            cancellationToken);

        var result = portfolios.Select(portfolio => new PortfolioBasicInformationResponse(
            portfolio.PortfolioId,
            portfolio.NitApprovedPortfolio,
            portfolio.VerificationDigit,
            portfolio.Name
        )).ToList();

        return Result.Success<IReadOnlyCollection<PortfolioBasicInformationResponse>>(result);
    }
}

