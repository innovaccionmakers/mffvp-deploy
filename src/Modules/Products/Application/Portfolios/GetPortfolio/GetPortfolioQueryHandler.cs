using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Rules;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios.GetPortfolio;
using Products.Integrations.Portfolios;

namespace Products.Application.Portfolios.GetPortfolio;

internal sealed class GetPortfolioQueryHandler(
    IPortfolioRepository portfolioRepository,
    IRuleEvaluator ruleEvaluator)
    : IQueryHandler<GetPortfolioQuery, PortfolioResponse>
{
    public async Task<Result<PortfolioResponse>> Handle(GetPortfolioQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await portfolioRepository.GetAsync(request.PortfolioId, cancellationToken);
        
        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(
                workflow: "Products.Portfolio.Validation",
                input   : portfolio,
                cancellationToken);
        
        if (!isValid)
        {
            var first = errors.First();
            return Result.Failure<PortfolioResponse>(
                Error.Validation(first.Code, first.Message));
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