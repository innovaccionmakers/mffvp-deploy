using Common.SharedKernel.Application.Messaging;

namespace DataSync.Integrations.TrustSync;

public sealed record TrustSyncCommand(DateTime ClosingDate, int PortfolioId) : ICommand<bool>;
