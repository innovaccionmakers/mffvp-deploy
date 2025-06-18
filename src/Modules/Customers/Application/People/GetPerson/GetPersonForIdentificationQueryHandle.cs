using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Integrations.People.GetPerson;
using Customers.Application.Abstractions;
using Common.SharedKernel.Application.Rules;
using Customers.Integrations.People;
using Application.People.GetPerson;

namespace Customers.Application.People.GetPerson;

public class GetPersonForIdentificationQueryHandle(
    IRuleEvaluator<CustomersModuleMarker> ruleEvaluator,
    GetPersonQueryHandlerValidation validator) : IQueryHandler<GetPersonForIdentificationQuery, PersonResponse>
{
    private const string ValidationWorkflow = "Customers.Person.ValidationAssociate";

    public async Task<Result<PersonResponse>> Handle(GetPersonForIdentificationQuery request,
        CancellationToken cancellationToken)
    {
        var validationContext = await validator.ValidateAsync(request, cancellationToken);

        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(
                ValidationWorkflow,
                validationContext,
                cancellationToken);

        if (!isValid)
        {
            var first = errors.First();
            return Result.Failure<PersonResponse>(
                Error.Validation(first.Code, first.Message));
        }

        // Si todo está bien, devolver la persona encontrada
        var person = validationContext.Person!;

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