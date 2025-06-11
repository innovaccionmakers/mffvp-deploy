using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Integrations.People.GetPerson;
using People.Application.Abstractions;
using People.Application.Abstractions.Rules;
using People.Domain.ConfigurationParameters;
using People.Domain.People;
using People.Integrations.People;

namespace People.Application.People.GetPerson;

public class GetPersonForIdentificationQueryHandle(
    IPersonRepository personRepository,
    IRuleEvaluator<PeopleModuleMarker> ruleEvaluator,
    IConfigurationParameterRepository configurationParameterRepository) : IQueryHandler<GetPersonForIdentificationQuery, PersonResponse>
{
    private const string ValidationWorkflow = "People.Person.ValidationAssociate";

    public async Task<Result<PersonResponse>> Handle(GetPersonForIdentificationQuery request,
        CancellationToken cancellationToken)
    {
        var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.IdentificationType, HomologScope.Of<GetPersonForIdentificationQuery>(c => c.IdentificationType), cancellationToken);

        var person =
            await personRepository.GetForIdentificationAsync(configurationParameter!.Uuid, request.Identification,
                cancellationToken);

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
            person!.PersonId,
            person.IdentificationType,
            person.HomologatedCode,
            person.Identification,
            person.FirstName,
            person.MiddleName,
            person.LastName,
            person.SecondLastName,
            person.Mobile,
            person.FullName,
            person.GenderId,
            person.CountryOfResidenceId,
            person.DepartmentId,
            person.MunicipalityId,
            person.Email,
            person.EconomicActivityId,
            person.Status,
            person.Address,
            person.IsDeclarant,
            person.InvestorTypeId,
            person.RiskProfileId
        );
        return response;
    }
}