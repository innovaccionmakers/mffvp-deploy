using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.TrustSync;

[AuditLog]
public sealed record TrustSyncCommand(
    int TrustId,
    int PortfolioId,
    DateTime ClosingDate,
    decimal PreClosingBalance,
    decimal Capital,
    decimal ContingentWithholding) : ICommand<bool>;
