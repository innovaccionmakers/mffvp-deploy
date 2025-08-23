
using Common.SharedKernel.Application.Messaging;

namespace DataSync.Integrations.TrustSync;

public sealed record TrustSyncPostCommand(DateTime ClosingDate, int PortfolioId) : ICommand<bool>;
