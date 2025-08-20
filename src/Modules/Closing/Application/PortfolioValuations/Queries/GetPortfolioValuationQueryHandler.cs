using Closing.Application.Abstractions;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Closing.Integrations.PortfolioValuations.Queries;
using Closing.Integrations.PortfolioValuations.Response;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;

namespace Closing.Application.PortfolioValuations.Queries;

public class GetPortfolioValuationQueryHandler(
    IPortfolioValuationRepository portfolioValuationRepository,
    ITrustYieldRepository trustYieldRepository,
    IYieldRepository yieldRepository,
    IRuleEvaluator<ClosingModuleMarker> ruleEvaluator) : IQueryHandler<GetPortfolioValuationQuery, IReadOnlyCollection<PortfolioValuationResponse>>
{
    private const string ValidationWorkflow = "Closing.GetPortfolioValuation.Validation";
    public async Task<Result<IReadOnlyCollection<PortfolioValuationResponse>>> Handle(GetPortfolioValuationQuery request, CancellationToken cancellationToken)
    {
        var portfolioValuations = await portfolioValuationRepository.GetPortfolioValuationsByClosingDateAsync(request.ClosingDate, cancellationToken);

        var contextValidation = new
        {
            PortfolioValuationsExists = portfolioValuations.Count != 0,
        };

        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(
                ValidationWorkflow,
                contextValidation,
                cancellationToken);

        if (!isValid)
        {
            var first = errors.First();
            return Result.Failure<IReadOnlyCollection<PortfolioValuationResponse>>(
                Error.Validation(first.Code, first.Message));
        }


        var portfolioTrustIds = await trustYieldRepository.GetTrustIdsByPortfolioAsync(request.ClosingDate, cancellationToken);
        var trustIdsByPortfolio = portfolioTrustIds.ToDictionary(p => p.PortfolioId);

        var yields = await yieldRepository.GetByClosingDateAsync(request.ClosingDate, cancellationToken);
        var yieldsByPortfolio = yields.ToDictionary(y => y.PortfolioId);

        var result = portfolioValuations.Select(x =>
        {
            var trustIds = trustIdsByPortfolio.GetValueOrDefault(x.PortfolioId)?.TrustIds ?? Array.Empty<long>();
            var yield = yieldsByPortfolio.GetValueOrDefault(x.PortfolioId);
            return new PortfolioValuationResponse(
                x.PortfolioId,
                x.ClosingDate,
                x.IncomingOperations,
                x.OutgoingOperations,
                yield?.Income ?? 0,
                yield?.Expenses ?? 0,
                yield?.Commissions ?? 0,
                yield?.Costs ?? 0,
                yield?.CreditedYields ?? 0,
                x.GrossYieldPerUnit,
                x.CostPerUnit,
                x.UnitValue,
                x.Units,
                x.Amount,
                trustIds
            );
        }).ToList();

        return Result.Success<IReadOnlyCollection<PortfolioValuationResponse>>(result);
    }
}
