using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Domain;
using DotNetCore.CAP;
using People.Application.Abstractions;
using People.Application.Abstractions.Rules;
using People.Domain.People;

namespace Infrastructure.EventBus
{
    public class PersonDataRequestConsumer(
    IPersonRepository personRepository,
    IRuleEvaluator<PeopleModuleMarker> ruleEvaluator,
    ICapPublisher capPublisher) : ICapSubscribe
    {
        private const string ValidationWorkflow = "People.Person.Validation";

        [CapSubscribe(nameof(PersonDataRequestEvent))]
        public async Task HandleRequest(PersonDataRequestEvent request, CancellationToken cancellationToken)
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
                Result.Failure<PersonDataResponseEvent>(
                    Error.Validation(first.Code, first.Message));

                await capPublisher.PublishAsync(nameof(PersonDataResponseEvent), first);
                return;
            }

            var responseEvent = new PersonDataResponseEvent(
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
            person.EconomicActivityId);

            await capPublisher.PublishAsync(nameof(PersonDataResponseEvent), responseEvent);

        }
    }
}