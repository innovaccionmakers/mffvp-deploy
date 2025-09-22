using Common.SharedKernel.Application.Messaging;

namespace Customers.Integrations.People.GetPeopleByIdentifications;

public sealed record GetPeopleByIdentificationsRequestQuery(IEnumerable<string> Identifications) : IQuery<IReadOnlyCollection<GetPeopleByIdentificationsResponse>>;
