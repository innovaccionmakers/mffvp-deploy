using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Application.Abstractions;
using Common.SharedKernel.Application.Rules;
using Customers.Domain.ConfigurationParameters;
using Customers.Integrations.DocumentTypes.GetDocumentTypeId;

namespace Customers.Application.DocumentTypes.GetDocumentTypeId;

internal sealed class GetDocumentTypeIdHandler(
    IConfigurationParameterRepository configurationParameterRepository,
    IRuleEvaluator<CustomersModuleMarker> ruleEvaluator)
    : IQueryHandler<GetDocumentTypeIdQuery, GetDocumentTypeIdResponse>
{
    private const string DocumentTypeValidationWorkflow = "Customers.Person.ValidateIdType";

    public async Task<Result<GetDocumentTypeIdResponse>> Handle(
        GetDocumentTypeIdQuery query,
        CancellationToken cancellationToken)
    {
        var documentType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            query.TypeIdHomologationCode,
            HomologScope.Of<GetDocumentTypeIdQuery>(c => c.TypeIdHomologationCode),
            cancellationToken);

        var validationContext = new { DocumentTypeExists = documentType is not null };

        var (ok, _, errors) = await ruleEvaluator.EvaluateAsync(
            DocumentTypeValidationWorkflow,
            validationContext,
            cancellationToken);

        if (!ok)
        {
            var first = errors.First();
            return Result.Failure<GetDocumentTypeIdResponse>(
                Error.Validation(first.Code, first.Message));
        }

        return Result.Success(new GetDocumentTypeIdResponse(documentType!.ConfigurationParameterId));
    }
}