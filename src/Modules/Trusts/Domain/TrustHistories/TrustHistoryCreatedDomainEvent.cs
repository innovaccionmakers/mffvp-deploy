using Common.SharedKernel.Domain;

namespace Trusts.Domain.TrustHistories;

public sealed class TrustHistoryCreatedDomainEvent(long trusthistoryId) : DomainEvent
{
    public long TrustHistoryId { get; } = trusthistoryId;
}