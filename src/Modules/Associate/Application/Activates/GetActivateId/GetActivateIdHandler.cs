using Associate.Application.Abstractions;
using Associate.Application.Abstractions.Rules;
using Associate.Domain.Activates;
using Associate.Integrations.Activates.GetActivateId;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Application.Activates.GetActivateId;

internal sealed class GetActivateIdHandler(
    IActivateRepository                       activateRepository,
    IRuleEvaluator<AssociateModuleMarker>     ruleEvaluator
) : IQueryHandler<GetActivateIdQuery, GetActivateIdResponse>
{
    private const string ActivateValidationWorkflow = "Associate.GetActivateId.Validation";

    public async Task<Result<GetActivateIdResponse>> Handle(
        GetActivateIdQuery query,
        CancellationToken cancellationToken)
    {
        var activate = await activateRepository
            .GetByIdTypeAndNumber(query.IdentificationType, query.Identification);
        
        var validationContext = new { ActivateExists = activate is not null };
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
            activate.Pensioner
            ));
    }
}