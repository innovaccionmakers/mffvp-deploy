using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.Trusts.CreateTrust;
public sealed record CreateTrustCommand(
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    decimal? TotalUnits,
    decimal Principal,
    decimal Earnings,
    int TaxCondition,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableBalance
) : ICommand<TrustResponse>;