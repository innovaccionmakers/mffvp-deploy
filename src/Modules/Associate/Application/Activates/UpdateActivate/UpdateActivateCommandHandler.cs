using Associate.Domain.ConfigurationParameters;
using System.Data.Common;
using Associate.Application.Abstractions;
using Associate.Application.Abstractions.Data;
using Associate.Application.Abstractions.Rules;
using Associate.Application.Activates.UpdateActivate;
using Associate.Domain.Activates;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Associate.Integrations.Activates;
using Associate.Integrations.Activates.UpdateActivate;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Application.Activates;

internal sealed class UpdateActivateCommandHandler(
    IActivateRepository activateRepository,
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork,
    IConfigurationParameterRepository configurationParameterRepository)
    : ICommandHandler<UpdateActivateCommand>
{
    private const string Workflow = "Associate.Activates.UpdateValidation";

    public async Task<Result> Handle(UpdateActivateCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);        
        var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.IdentificationType, HomologScope.Of<UpdateActivateCommand>(c => c.IdentificationType), cancellationToken);
        Activate existingActivate = await activateRepository.GetByIdTypeAndNumber(configurationParameter.Uuid, request.Identification, cancellationToken);

        
        var validationContext = new ActivateUpdateValidationContext(request, existingActivate!, configurationParameter.Uuid);

        var (isValid, _, ruleErrors) =
            await ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var first = ruleErrors
                .OrderByDescending(r => r.Code)
                .First();

            return Result.Failure<ActivateResponse>(
                Error.Validation(first.Code, first.Message));
        }

        existingActivate.UpdateDetails(
            request.Pensioner
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("Actualización de Condición Pensionado Exitosa");
    }
}