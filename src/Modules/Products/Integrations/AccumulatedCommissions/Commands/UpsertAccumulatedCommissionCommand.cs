using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.AccumulatedCommissions.Commands;

[AuditLog]
public sealed record UpsertAccumulatedCommissionCommand(
    int PortfolioId,
    int CommissionId,
    decimal AccumulatedValue,
    DateTime CloseDate
) : ICommand;