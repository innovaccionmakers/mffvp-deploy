using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.TrustHistories.GetTrustHistories;

public sealed record GetTrustHistoriesQuery : IQuery<IReadOnlyCollection<TrustHistoryResponse>>;