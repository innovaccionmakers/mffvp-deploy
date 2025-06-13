using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Integrations.People.GetPerson;
using Customers.Domain.ConfigurationParameters;
using Customers.Application.Abstractions;
using Common.SharedKernel.Application.Rules;
using Customers.Domain.People;
using Customers.Integrations.People;

namespace Customers.Application.People.GetPerson;

public class GetPersonForIdentificationQueryHandle(
    IPersonRepository personRepository,
    IRuleEvaluator<CustomersModuleMarker> ruleEvaluator,
    IConfigurationParameterRepository configurationParameterRepository) : IQueryHandler<GetPersonForIdentificationQuery, PersonResponse>
{
    private const string ValidationWorkflow = "Customers.Person.ValidationAssociate";

    public async Task<Result<PersonResponse>> Handle(GetPersonForIdentificationQuery request,
        CancellationToken cancellationToken)
    {
        var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.DocumentType, HomologScope.Of<GetPersonForIdentificationQuery>(c => c.DocumentType), cancellationToken);
        Guid uuid = configurationParameter == null ? new Guid() : configurationParameter.Uuid;

        var person =
            await personRepository.GetForIdentificationAsync(uuid, request.Identification,
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
            person.DocumentType,
            person.HomologatedCode,
            person.Identification,
            person.FirstName,
            person.MiddleName,
            person.LastName,
            person.SecondLastName,
            person.BirthDate,
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