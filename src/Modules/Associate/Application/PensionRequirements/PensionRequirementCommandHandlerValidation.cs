
using Associate.Integrations.Activates.GetActivateId;
using Common.SharedKernel.Domain;
using MediatR;
using Common.SharedKernel.Application.Rules;
using Associate.Application.Abstractions;

namespace Application.PensionRequirements;

public class PensionRequirementCommandHandlerValidation(
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    ISender sender)
{
    public async Task<Result> ValidateRequestAsync<TCommand>(
            TCommand request,
            string identificationType,
            string identification,
            string Workflow,
            Func<TCommand, GetActivateIdResponse, object> validationContextFactory,
            CancellationToken cancellationToken)
    {
        var activateQuery = new GetActivateIdQuery(identificationType, identification);
        var activateResult = await sender.Send(activateQuery, cancellationToken);

        if (activateResult.IsFailure)
            return Result.Failure(
                Error.Validation(activateResult.Error.Code ?? string.Empty, activateResult.Error.Description ?? string.Empty));

        var validationContext = validationContextFactory(request, activateResult.Value);

        var (isValid, _, ruleErrors) =
            await ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var first = ruleErrors
                .OrderByDescending(r => r.Code)
                .First();

            return Result.Failure(
                Error.Validation(first.Code, first.Message));
        }

        return Result.Success();
    }
}
