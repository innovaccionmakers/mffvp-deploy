using Common.SharedKernel.Application.Messaging;
using Customers.Domain.People;

namespace Customers.Integrations.People.GetPersons;

public sealed record class GetPersonsByDocumentsQuery(IReadOnlyCollection<(Guid DocumentTypeUuid, string Identification)> Documents) : IQuery<IReadOnlyCollection<PersonResponse>>;
