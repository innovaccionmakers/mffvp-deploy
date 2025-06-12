using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.ProfitLosses.GetProfitandLoss;

public sealed record GetProfitandLossQuery(
    int PortfolioId,
    DateTime EffectiveDate
) : IQuery<GetProfitandLossResponse>;