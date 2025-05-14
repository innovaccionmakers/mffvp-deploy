namespace Trusts.Integrations.TrustHistories;

public sealed record TrustHistoryResponse(
    long TrustHistoryId,
    long TrustId,
    decimal Earnings,
    DateTime Date,
    string SalesUserId
);