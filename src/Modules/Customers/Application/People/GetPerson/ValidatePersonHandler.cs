using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Application.Abstractions;
using Customers.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Customers.Domain.People;
using Customers.Integrations.People;
using Customers.Integrations.People.GetPerson;

namespace Customers.Application.People.GetPerson;

internal sealed class ValidatePersonHandler(
    IConfigurationParameterRepository configurationParameterRepository,
    IPersonRepository personRepository,
    IRuleEvaluator<CustomersModuleMarker> ruleEvaluator
) : IQueryHandler<ValidatePersonQuery, ValidatePersonResponse>
{
    private const string DocumentTypeValidationWorkflow = "Customers.Person.Validation";

    public async Task<Result<ValidatePersonResponse>> Handle(
        ValidatePersonQuery query,
        CancellationToken cancellationToken)
    {
        var documentTypeCode = query.DocumentTypeHomologatedCode;
        var identification = query.IdentificationNumber;

        var documentType = await configurationParameterRepository
            .GetByCodeAndScopeAsync(
                documentTypeCode,
                HomologScope.Of<ValidatePersonQuery>(c => c.DocumentTypeHomologatedCode),
                cancellationToken);

        var person = await personRepository
            .GetByIdentificationAsync(
                identification,
                documentType?.Name,
                cancellationToken);

        var validationContext = new
        {
            DocumentTypeExists = documentType is not null,
            PersonExists = person is not null,
            PersonIsActive = person?.Status ?? Status.Inactive
        };

        var (rulesOk, _, ruleErrors) = await ruleEvaluator
            .EvaluateAsync(DocumentTypeValidationWorkflow, validationContext, cancellationToken);

        if (!rulesOk)
        {
            var first = ruleErrors.First();
            return Result.Failure<ValidatePersonResponse>(
                Error.Validation(first.Code, first.Message));
        }

        return Result.Success(new ValidatePersonResponse(
            true));
    }
}