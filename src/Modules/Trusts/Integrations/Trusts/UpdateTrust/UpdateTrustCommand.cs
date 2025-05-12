using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.UpdateTrust;

public sealed record UpdateTrustCommand(
    Guid TrustId,
    int NewAffiliateId,
    int NewClientId,
    int NewObjectiveId,
    int NewPortfolioId,
    decimal NewTotalBalance,
    int NewTotalUnits,
    decimal NewPrincipal,
    decimal NewEarnings,
    int NewTaxCondition,
    int NewContingentWithholding
) : ICommand<TrustResponse>;