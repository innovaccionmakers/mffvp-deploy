using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios.GetPortfolio;
using Products.Integrations.Portfolios;

namespace Products.Application.Portfolios.GetPortfolio;

internal sealed class GetPortfolioQueryHandler(
    IPortfolioRepository portfolioRepository)
    : IQueryHandler<GetPortfolioQuery, PortfolioResponse>
{
    public async Task<Result<PortfolioResponse>> Handle(GetPortfolioQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await portfolioRepository.GetAsync(request.PortfolioId, cancellationToken);
        if (portfolio is null)
        {
            return Result.Failure<PortfolioResponse>(PortfolioErrors.NotFound(request.PortfolioId));
        }
        var response = new PortfolioResponse(
            portfolio.PortfolioId,
            portfolio.StandardCode,
            portfolio.Name,
            portfolio.ShortName,
            portfolio.ModalityId,
            portfolio.InitialMinimumAmount
        );
        return response;
    }
}