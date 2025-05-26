using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Integrations.People.GetPerson;
using People.Application.Abstractions;
using People.Application.Abstractions.Rules;
using People.Domain.People;
using People.Integrations.People;

namespace People.Application.People.GetPerson
{
    public class GetPersonForIdentificationQueryHandle(
    IPersonRepository personRepository,
    IRuleEvaluator<PeopleModuleMarker> ruleEvaluator) : IQueryHandler<GetPersonForIdentificationQuery, PersonResponse>
    {
        private const string ValidationWorkflow = "People.Person.Validation";

        public async Task<Result<PersonResponse>> Handle(GetPersonForIdentificationQuery request, CancellationToken cancellationToken)
        {
            var person = await personRepository.GetForIdentificationAsync(request.DocumentType, request.Identification, cancellationToken);

            var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(
                ValidationWorkflow,
                person,
                cancellationToken);

            if (!isValid)
            {
                var first = errors.First();
                return Result.Failure<PersonResponse>(
                    Error.Validation(first.Code, first.Message));
            }

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
}