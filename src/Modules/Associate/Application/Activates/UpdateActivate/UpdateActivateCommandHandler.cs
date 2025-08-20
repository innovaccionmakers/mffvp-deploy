using Application.Activates;

using Associate.Application.Abstractions;
using Associate.Application.Abstractions.Data;
using Associate.Integrations.Activates;
using Associate.Integrations.Activates.UpdateActivate;

using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using System.Data.Common;

namespace Associate.Application.Activates;

internal sealed class UpdateActivateCommandHandler(
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork,
    ActivatesCommandHandlerValidation validator)
    : ICommandHandler<UpdateActivateCommand>
{
    private const string Workflow = "Associate.Activates.UpdateValidation";
    public async Task<Result> Handle(UpdateActivateCommand request, CancellationToken cancellationToken)
    {       
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var validationResults = await validator.UpdateActivateValidationContext(request, cancellationToken);

        var (isValid, _, ruleErrors) =
            await ruleEvaluator.EvaluateAsync(Workflow, validationResults, cancellationToken);

        if (!isValid)
        {
            var first = ruleErrors
                .OrderByDescending(r => r.Code)
                .First();

            return Result.Failure<ActivateResponse>(
                Error.Validation(first.Code, first.Message));
        }

        validationResults.ExistingActivate.UpdateDetails(
            request.Pensioner ?? false
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("Actualización de Condición Pensionado Exitosa");
    }
}