using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustHistories.UpdateTrustHistory;

public sealed record UpdateTrustHistoryCommand(
    long TrustHistoryId,
    long NewTrustId,
    decimal NewEarnings,
    DateTime NewDate,
    string NewSalesUserId
) : ICommand<TrustHistoryResponse>;