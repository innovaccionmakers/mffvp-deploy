using Closing.Application.Abstractions;
using Closing.Domain.PortfolioValuations;
using Closing.Integrations.PortfolioValuations.Queries;
using Closing.Integrations.PortfolioValuations.Response;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using System.Linq;

namespace Closing.Application.PortfolioValuations.Queries;

internal sealed class GetPortfolioValuationInfoQueryHandler(
    IPortfolioValuationRepository portfolioValuationRepository,
    IInternalRuleEvaluator<ClosingModuleMarker> ruleEvaluator)
    : IQueryHandler<GetPortfolioValuationInfoQuery, PortfolioValuationInfoResponse>
{
    private const string WorkflowName = "Closing.GetPortfolioValuationInfo.Validation";

    public async Task<Result<PortfolioValuationInfoResponse>> Handle(
        GetPortfolioValuationInfoQuery request,
        CancellationToken cancellationToken)
    {
        var valuation = await portfolioValuationRepository
            .GetReadOnlyByPortfolioAndDateAsync(request.PortfolioId, request.ClosingDate, cancellationToken);

        var ruleContext = new
        {
            ValuationExists = valuation is not null,
            UnitValueIsPositive = valuation is not null && valuation.UnitValue > 0
        };

        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(WorkflowName, ruleContext, cancellationToken);

        if (!isValid)
        {
            var firstError = errors.First();

            return Result.Failure<PortfolioValuationInfoResponse>(
                Error.Validation(firstError.Code, firstError.Message));
        }

        var response = new PortfolioValuationInfoResponse(
            valuation!.PortfolioId,
            valuation.ClosingDate,
            valuation.UnitValue);

        return Result.Success(response, "Consulta realizada correctamente");
    }
}
