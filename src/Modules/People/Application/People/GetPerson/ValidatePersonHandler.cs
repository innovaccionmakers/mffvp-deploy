using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Application.Abstractions;
using People.Application.Abstractions.Rules;
using People.Domain.ConfigurationParameters;
using People.Domain.People;
using People.Integrations.People;
using People.Integrations.People.GetPerson;

namespace People.Application.People.GetPerson;

internal sealed class ValidatePersonHandler(
    IConfigurationParameterRepository configurationParameterRepository,
    IPersonRepository personRepository,
    IRuleEvaluator<PeopleModuleMarker> ruleEvaluator
) : IQueryHandler<ValidatePersonQuery, ValidatePersonResponse>
{
    private const string DocumentTypeValidationWorkflow = "People.Person.Validation";

    public async Task<Result<ValidatePersonResponse>> Handle(
        ValidatePersonQuery query,
        CancellationToken cancellationToken)
    {
        var documentTypeCode = query.DocumentTypeHomologatedCode;
        var identification = query.IdentificationNumber;

        var documentType = await configurationParameterRepository
            .GetByHomologationCodeAsync(documentTypeCode,  cancellationToken);

        var person = await personRepository
            .GetByIdentificationAsync(identification, documentType.Name, cancellationToken);

        var validationContext = new
        {
            DocumentTypeExists     = documentType != null,
            PersonExists           = person != null
        };

        var (validationSucceeded, _, validationErrors) = await ruleEvaluator
            .EvaluateAsync(DocumentTypeValidationWorkflow, validationContext, cancellationToken);

        if (!validationSucceeded)
        {
            var firstError = validationErrors.First();
            return Result.Failure<ValidatePersonResponse>(
                Error.Validation(firstError.Code, firstError.Message));
        }

        return Result.Success(new ValidatePersonResponse(true));
    }
}