using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.TrustInfo;

namespace Trusts.Application.TrustInfo;

internal sealed class TrustInfoQueryHandler(
    ITrustRepository trustRepository,
    IInternalRuleEvaluator<TrustsModuleMarker> ruleEvaluator)
    : IQueryHandler<TrustInfoQuery, TrustInfoQueryResponse>
{
    private const string WorkflowName = "Trusts.TrustInfo.Validation";

    public async Task<Result<TrustInfoQueryResponse>> Handle(
        TrustInfoQuery request,
        CancellationToken cancellationToken)
    {
        var trust = await trustRepository
            .GetByClientOperationIdAsync(request.ClientOperationId, cancellationToken);

        var normalizedBalance = decimal.Round(trust?.TotalBalance ?? 0m, 2, MidpointRounding.AwayFromZero);
        var normalizedContribution = decimal.Round(request.ContributionValue, 2, MidpointRounding.AwayFromZero);

        var ruleContext = new
        {
            TrustExists = trust is not null,
            TrustIsActive = trust?.Status == LifecycleStatus.Active,
            TrustTotalBalance = normalizedBalance,
            ContributionValue = normalizedContribution
        };

        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(WorkflowName, ruleContext, cancellationToken);

        if (!isValid)
        {
            var validationError = errors.First();
            return Result.Failure<TrustInfoQueryResponse>(
                Error.Validation(validationError.Code, validationError.Message));
        }

        return Result.Success(new TrustInfoQueryResponse(trust!.TrustId));
    }
}
