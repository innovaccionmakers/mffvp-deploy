using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.People;
using People.Integrations.People.GetPeople;
using People.Integrations.People;
using System.Collections.Generic;
using System.Linq;

namespace People.Application.People.GetPeople;

internal sealed class GetPeopleQueryHandler(
    IPersonRepository personRepository)
    : IQueryHandler<GetPeopleQuery, IReadOnlyCollection<PersonResponse>>
{
    public async Task<Result<IReadOnlyCollection<PersonResponse>>> Handle(GetPeopleQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await personRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new PersonResponse(
                e.PersonId,
                e.DocumentType,
                e.StandardCode,
                e.Identification,
                e.FirstName,
                e.MiddleName,
                e.LastName,
                e.SecondLastName,
                e.IssueDate,
                e.IssueCityId,
                e.BirthDate,
                e.BirthCityId,
                e.Mobile,
                e.FullName,
                e.MaritalStatusId,
                e.GenderId,
                e.CountryId,
                e.Email,
                e.EconomicActivityId))
            .ToList();

        return Result.Success<IReadOnlyCollection<PersonResponse>>(response);
    }
}