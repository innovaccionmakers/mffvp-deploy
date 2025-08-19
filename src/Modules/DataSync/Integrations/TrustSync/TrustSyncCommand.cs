using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace DataSync.Integrations.TrustSync;

[AuditLog]
public sealed record TrustSyncCommand(DateTime ClosingDate, int PortfolioId) : ICommand<bool>;
