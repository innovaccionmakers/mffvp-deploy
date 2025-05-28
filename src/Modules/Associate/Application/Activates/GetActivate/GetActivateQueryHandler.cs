using Associate.Application.Abstractions;
using Associate.Application.Abstractions.Rules;
using Associate.Domain.Activates;
using Associate.Integrations.Activates;
using Associate.Integrations.Activates.GetActivate;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;


namespace Associate.Application.Activates.GetActivate;

internal sealed class GetActivateQueryHandler(
    IActivateRepository activateRepository,
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator) : IQueryHandler<GetActivateQuery, ActivateResponse>
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

        var response = new ActivateResponse(activate.ActivateId, activate.IdentificationType, activate.Identification,
            activate.Pensioner, activate.MeetsPensionRequirements, activate.ActivateDate);

        return response;
    }
}