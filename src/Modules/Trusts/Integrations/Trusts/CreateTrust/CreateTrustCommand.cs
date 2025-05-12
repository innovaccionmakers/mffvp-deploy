using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.CreateTrust;

public sealed record CreateTrustCommand(
    int AffiliateId,
    int ClientId,
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    int TotalUnits,
    decimal Principal,
    decimal Earnings,
    int TaxCondition,
    int ContingentWithholding
) : ICommand<TrustResponse>;