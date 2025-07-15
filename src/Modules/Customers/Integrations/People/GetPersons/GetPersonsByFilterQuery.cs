using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;


namespace Customers.Integrations.People.GetPersons;

public sealed record class GetPersonsByFilterQuery(
    string? IdentificationType,
    SearchByType? SearchBy = null,
    string? Text = null) : IQuery<IReadOnlyCollection<PersonInformationResponse>>;    
