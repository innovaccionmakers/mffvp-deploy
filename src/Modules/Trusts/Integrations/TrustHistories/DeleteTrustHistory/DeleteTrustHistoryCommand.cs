using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustHistories.DeleteTrustHistory;

public sealed record DeleteTrustHistoryCommand(
    long TrustHistoryId
) : ICommand;