using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.DataSync.TrustSync;

public sealed record TrustSyncCommand(DateTime ClosingDate) : ICommand<bool>;

