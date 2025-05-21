using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using People.Application.Abstractions;
using People.Application.Abstractions.Rules;
using People.Domain.People;
using People.Integrations.People;
using People.Integrations.People.GetPerson;
using People.Integrations.People.GetPersonValidation;

namespace People.Application.People.GetPerson;

internal sealed class GetPersonQueryHandler(
    IPersonRepository personRepository,
    IRuleEvaluator<PeopleModuleMarker> ruleEvaluator,
    ICapRpcClient rpc,
    ILogger<GetPersonQueryHandler> log)
    : IQueryHandler<GetPersonQuery, PersonResponse>
{
    private const string ValidationWorkflow = "People.Person.Validation";

    public async Task<Result<PersonResponse>> Handle(GetPersonQuery request, CancellationToken cancellationToken)
    {
        var validation = await rpc.CallAsync<
            GetPersonValidationRequest,
            GetPersonValidationResponse>(
            "people.validation.request",
            new GetPersonValidationRequest(request.PersonId),
            TimeSpan.FromSeconds(5),
            cancellationToken);

        if (!validation.IsValid)
            return Result.Failure<PersonResponse>(
                Error.Validation(validation.Code!, validation.Message!));

        var person = await personRepository.GetAsync(request.PersonId, cancellationToken);

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