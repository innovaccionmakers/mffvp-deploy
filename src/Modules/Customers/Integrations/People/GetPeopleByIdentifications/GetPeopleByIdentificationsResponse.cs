namespace Customers.Integrations.People.GetPeopleByIdentifications;

public sealed record GetPeopleByIdentificationsResponse(
    string Identification,
    string DocumentType,
    string FullName
);
