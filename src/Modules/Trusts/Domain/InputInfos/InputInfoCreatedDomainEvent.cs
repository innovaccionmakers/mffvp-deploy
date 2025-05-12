using Common.SharedKernel.Domain;

namespace Trusts.Domain.InputInfos;

public sealed class InputInfoCreatedDomainEvent(Guid inputinfoId) : DomainEvent
{
    public Guid InputInfoId { get; } = inputinfoId;
}