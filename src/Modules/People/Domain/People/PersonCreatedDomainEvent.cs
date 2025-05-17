using Common.SharedKernel.Domain;

namespace People.Domain.People;

public sealed class PersonCreatedDomainEvent(long personId) : DomainEvent
{
    public long PersonId { get; } = personId;
}