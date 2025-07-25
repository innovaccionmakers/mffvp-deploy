using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.IntegrationEvents.PersonValidation;
using Products.Application.Abstractions;
using Common.SharedKernel.Application.Rules;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.GetPortfolio;

namespace Products.Application.Portfolios.GetPortfolio;

internal sealed class GetPortfolioQueryHandler(
    IPortfolioRepository portfolioRepository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator)
    : IQueryHandler<GetPortfolioQuery, PortfolioResponse>
{
    private const string ValidationWorkflow = "Products.Portfolio.Validation";

    public async Task<Result<PortfolioResponse>> Handle(GetPortfolioQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await portfolioRepository.GetAsync(request.PortfolioId, cancellationToken);

        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(
                ValidationWorkflow,
                portfolio,
                cancellationToken);

        if (!isValid)
        {
            var first = errors.First();
            return Result.Failure<PortfolioResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var response = new PortfolioResponse(
            portfolio.PortfolioId,
            portfolio.HomologatedCode,
            portfolio.Name,
            portfolio.ShortName,
            portfolio.ModalityId,
            portfolio.InitialMinimumAmount,
            portfolio.CurrentDate
        );
        return response;
    }
}