using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.People;
using People.Integrations.People.GetPerson;
using People.Integrations.People;

namespace People.Application.People.GetPerson;

internal sealed class GetPersonQueryHandler(
    IPersonRepository personRepository)
    : IQueryHandler<GetPersonQuery, PersonResponse>
{
    public async Task<Result<PersonResponse>> Handle(GetPersonQuery request, CancellationToken cancellationToken)
    {
        var person = await personRepository.GetAsync(request.PersonId, cancellationToken);
        if (person is null) return Result.Failure<PersonResponse>(PersonErrors.NotFound(request.PersonId));
        var response = new PersonResponse(
            person.PersonId,
            person.DocumentType,
            person.StandardCode,
            person.Identification,
            person.FirstName,
            person.MiddleName,
            person.LastName,
            person.SecondLastName,
            person.IssueDate,
            person.IssueCityId,
            person.BirthDate,
            person.BirthCityId,
            person.Mobile,
            person.FullName,
            person.MaritalStatusId,
            person.GenderId,
            person.CountryId,
            person.Email,
            person.EconomicActivityId
        );
        return response;
    }
}