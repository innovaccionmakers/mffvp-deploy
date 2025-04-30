using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.Trusts.UpdateTrust;
public sealed record UpdateTrustCommand(
    Guid TrustId,
    int NewAffiliateId,
    int NewObjectiveId,
    int NewPortfolioId,
    decimal NewTotalBalance,
    decimal? NewTotalUnits,
    decimal NewPrincipal,
    decimal NewEarnings,
    int NewTaxCondition,
    decimal NewContingentWithholding,
    decimal NewEarningsWithholding,
    decimal NewAvailableBalance
) : ICommand<TrustResponse>;