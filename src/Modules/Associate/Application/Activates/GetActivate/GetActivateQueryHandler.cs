using Associate.Application.Abstractions;
using Common.SharedKernel.Application.Rules;
using Associate.Domain.Activates;
using Associate.Integrations.Activates;
using Associate.Integrations.Activates.GetActivate;
using Associate.Integrations.ConfigurationParameters.GetConfigurationParameter;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;


namespace Associate.Application.Activates.GetActivate;

internal sealed class GetActivateQueryHandler(
    IActivateRepository activateRepository,
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    ISender sender) : IQueryHandler<GetActivateQuery, ActivateResponse>
{
    private const string ValidationWorkflow = "Associate.GetActivate.Validation";

    public async Task<Result<ActivateResponse>> Handle(GetActivateQuery request,
        CancellationToken cancellationToken)
    {
        var activate = await activateRepository.GetByIdAsync(request.ActivateId, cancellationToken);

        var contextValidation = new
        {
            ActiveExists = activate != null,
        };
        
        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(
                ValidationWorkflow,
                contextValidation,
                cancellationToken);
        
        if (!isValid)
        {
            var first = errors.First();
            return Result.Failure<ActivateResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var identificationType = new GetConfigurationParameterQuery(activate!.DocumentType);
        var identificationTypeResult = await sender.Send(identificationType, cancellationToken);

        var response = new ActivateResponse(activate.ActivateId, identificationTypeResult.Value.HomologationCode, activate.Identification,
            activate.Pensioner, activate.MeetsPensionRequirements, activate.ActivateDate);

        return response;
    }
}