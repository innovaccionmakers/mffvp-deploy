using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.TrustSync;

[AuditLog]
public sealed record TrustSyncCommand(
    int PortfolioId,
    DateTime ClosingDate
    ) : ICommand<bool>;
