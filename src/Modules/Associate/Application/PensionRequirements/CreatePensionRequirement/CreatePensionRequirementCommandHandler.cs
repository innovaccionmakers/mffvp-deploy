using Associate.Domain.ConfigurationParameters;
using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements;
using Associate.Application.Abstractions.Data;
using Application.PensionRequirements.CreatePensionRequirement;
using Application.PensionRequirements;
using Associate.Integrations.Activates.GetActivateId;
using Common.SharedKernel.Application.Attributes;

namespace Associate.Application.PensionRequirements.CreatePensionRequirement;


internal sealed class CreatePensionRequirementCommandHandler(
    IPensionRequirementRepository pensionrequirementRepository,
    IUnitOfWork unitOfWork,
    PensionRequirementCommandHandlerValidation validator,
    IConfigurationParameterRepository configurationParameterRepository)
    : ICommandHandler<CreatePensionRequirementCommand>
{
    private const string Workflow = "Associate.PensionRequirement.CreateValidation";

    public async Task<Result> Handle(CreatePensionRequirementCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        GetActivateIdResponse? activateData = null;
        
        var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.DocumentType, HomologScope.Of<CreatePensionRequirementCommand>(c => c.DocumentType), cancellationToken);
        Guid uuid = configurationParameter == null ? new Guid() : configurationParameter.Uuid;
        
        var validationResult = await validator.ValidateRequestAsync(
                request,
                request.DocumentType,
                request.Identification,
                Workflow,
                (cmd, activateResult) => {
                    activateData = activateResult;
                    return new CreatePensionRequirementValidationContext(cmd, activateResult, uuid);
                },
                cancellationToken);

        if (validationResult.IsFailure)
            return Result.Failure(
                Error.Validation(validationResult.Error.Code ?? string.Empty, validationResult.Error.Description ?? string.Empty));

        var activateId = activateData!.ActivateId;

        var result = PensionRequirement.Create(
            request.StartDateReqPen,
            request.EndDateReqPen,
            DateTime.UtcNow,
            Status.Active,
            activateId
        );

        if (result.IsFailure)
            return Result.Failure<PensionRequirementResponse>(result.Error);

        await pensionrequirementRepository.DeactivateExistingRequirementsAsync(activateId,  cancellationToken);

        pensionrequirementRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("Registro de Requisitos de Pensión Exitoso");
    }
}