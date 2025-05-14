using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustHistories.CreateTrustHistory;

public sealed record CreateTrustHistoryCommand(
    long TrustId,
    decimal Earnings,
    DateTime Date,
    string SalesUserId
) : ICommand<TrustHistoryResponse>;