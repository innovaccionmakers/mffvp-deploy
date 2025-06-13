using Associate.Application.Abstractions;
using Associate.Domain.Activates;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.Activates.GetActivateId;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;

namespace Associate.Application.Activates.GetActivateId;

internal sealed class GetActivateIdHandler(
    IActivateRepository activateRepository,
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IConfigurationParameterRepository configurationParameterRepository
) : IQueryHandler<GetActivateIdQuery, GetActivateIdResponse>
{
    private const string ActivateValidationWorkflow = "Associate.GetActivateId.Validation";

    public async Task<Result<GetActivateIdResponse>> Handle(
        GetActivateIdQuery query,
        CancellationToken cancellationToken)
    {
        var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            query.DocumentType,
            HomologScope.Of<GetActivateIdResponse>(c => c.DocumentType),
            cancellationToken);
        Guid uuid = configurationParameter == null ? new Guid() : configurationParameter.Uuid;

        Activate? activate = null;
        if (configurationParameter is not null)
            activate = await activateRepository
                .GetByIdTypeAndNumber(uuid, query.Identification, cancellationToken);

        var validationContext = new
        {
            documentType = configurationParameter,
            ActivateExists = activate is not null
        };
        var (ok, _, errors) = await ruleEvaluator
            .EvaluateAsync(ActivateValidationWorkflow, validationContext, cancellationToken);

        if (!ok)
        {
            var first = errors.First();
            return Result.Failure<GetActivateIdResponse>(
                Error.Validation(first.Code, first.Message));
        }

        return Result.Success(new GetActivateIdResponse(
            activate!.ActivateId,
            uuid,
            activate.Pensioner
        ));
    }
}