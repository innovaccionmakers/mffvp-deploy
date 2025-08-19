using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.ProfitLosses.ProfitandLossLoad;

[AuditLog]
public sealed record ProfitandLossLoadCommand(
    int PortfolioId,
    DateTime EffectiveDate,
    IReadOnlyDictionary<string, decimal> ConceptAmounts
) : ICommand<bool>;