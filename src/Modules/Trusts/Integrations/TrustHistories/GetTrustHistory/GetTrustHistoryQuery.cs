using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustHistories.GetTrustHistory;

public sealed record GetTrustHistoryQuery(
    long TrustHistoryId
) : IQuery<TrustHistoryResponse>;