using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.CreateTrust;

public sealed record CreateTrustCommand(
    int AffiliateId,
    int ClientId,
    DateTime CreationDate,
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    int TotalUnits,
    decimal Principal,
    decimal Earnings,
    int TaxCondition,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableAmount,
    decimal ContingentWithholdingPercentage
) : ICommand<TrustResponse>;