namespace Customers.Integrations.People.GetPeopleByIdentifications;

public sealed record GetPeopleByIdentificationsResponse(
    string Identification,
    string FullName
);
