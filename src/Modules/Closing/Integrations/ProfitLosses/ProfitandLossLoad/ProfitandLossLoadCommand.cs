using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.ProfitLosses.ProfitandLossLoad;

public sealed record ProfitandLossLoadCommand(
    int PortfolioId,
    DateTime EffectiveDate,
    decimal GrossReturns,
    decimal Expenses
) : ICommand<bool>;