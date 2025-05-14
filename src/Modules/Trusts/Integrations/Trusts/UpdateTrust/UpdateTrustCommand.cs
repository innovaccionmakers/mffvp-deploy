using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.UpdateTrust;

public sealed record UpdateTrustCommand(
    long TrustId,
    int NewAffiliateId,
    int NewClientId,
    DateTime NewCreationDate,
    int NewObjectiveId,
    int NewPortfolioId,
    decimal NewTotalBalance,
    int NewTotalUnits,
    decimal NewPrincipal,
    decimal NewEarnings,
    int NewTaxCondition,
    decimal NewContingentWithholding,
    decimal NewEarningsWithholding,
    decimal NewAvailableAmount,
    decimal NewContingentWithholdingPercentage
) : ICommand<TrustResponse>;