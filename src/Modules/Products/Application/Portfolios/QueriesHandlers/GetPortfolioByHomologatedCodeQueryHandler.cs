using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;
using Common.SharedKernel.Core.Primitives;


namespace Products.Application.Portfolios.QueriesHandlers
{
    internal sealed class GetPortfolioByHomologatedCodeQueryHandler(IPortfolioRepository portfolioRepository,
                                                                    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator) : IQueryHandler<GetPortfolioByHomologatedCodeQuery, PortfolioResponse>
    {
        private const string ValidationWorkflow = "Products.Portfolio.Validation";

        public async Task<Result<PortfolioResponse>> Handle(GetPortfolioByHomologatedCodeQuery request, CancellationToken cancellationToken)
        {
            var portfolio = await portfolioRepository.GetByHomologatedCodeAsync(request.HomologatedCode, cancellationToken);

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
}
