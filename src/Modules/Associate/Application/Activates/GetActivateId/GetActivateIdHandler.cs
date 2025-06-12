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
            query.IdentificationType,
            HomologScope.Of<GetActivateIdResponse>(c => c.IdentificationType),
            cancellationToken);

        Activate? activate = null;
        if (configurationParameter is not null)
            activate = await activateRepository
                .GetByIdTypeAndNumber(configurationParameter.Uuid, query.Identification, cancellationToken);

        var validationContext = new
        {
            identificationType = configurationParameter,
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
            configurationParameter.Uuid,
            activate.Pensioner
        ));
    }
}