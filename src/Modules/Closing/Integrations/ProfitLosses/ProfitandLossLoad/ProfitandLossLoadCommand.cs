using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.ProfitLosses.ProfitandLossLoad;

public sealed record ProfitandLossLoadCommand(
    int PortfolioId,
    DateTime EffectiveDate,
    IReadOnlyDictionary<string, decimal> ConceptAmounts
) : ICommand<bool>;