using Common.SharedKernel.Domain;

namespace Customers.Domain.People;

public sealed class PersonCreatedDomainEvent(long personId) : DomainEvent
{
    public long PersonId { get; } = personId;
}