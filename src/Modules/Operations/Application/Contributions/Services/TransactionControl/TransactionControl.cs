using Microsoft.Extensions.Logging;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Integrations.Contributions.CreateContribution;
using System.Text.Json;

namespace Operations.Application.Contributions.TransactionControl;

public sealed class TransactionControl(
    IClientOperationRepository clientOperationRepository,
    IAuxiliaryInformationRepository auxiliaryInformationRepository,
    IOperationCompleted operationCompleted,
    IUnitOfWork unitOfWork,
    ITaxCalculator taxCalculator,
    ILogger<TransactionControl> logger)
    : ITransactionControl
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private const string ClassName = nameof(TransactionControl);
    public async Task<(ClientOperation Operation, TaxResult Tax)> ExecuteAsync(
        CreateContributionCommand command,
        PrevalidationResult prevalidationResult,
        CancellationToken cancellationToken)
    {

        var isCertified = command.CertifiedContribution?.Trim().ToUpperInvariant() == "SI";
        var tax = await taxCalculator.ComputeAsync(
            prevalidationResult.AffiliateActivation.Item3,
            isCertified,
            command.Amount,
            cancellationToken);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var operation = ClientOperation.Create(
            DateTime.UtcNow,
            prevalidationResult.RemoteData.AffiliateId,
            prevalidationResult.RemoteData.ObjectiveId,
            prevalidationResult.RemoteData.PortfolioId,
            command.Amount,
            DateTime.SpecifyKind(command.ExecutionDate, DateTimeKind.Utc),
            prevalidationResult.Catalogs.Subtype?.OperationTypeId ?? 0,
            DateTime.UtcNow).Value;
        clientOperationRepository.Insert(operation);

        logger.LogInformation("{Class} - Operaci�n creada e insertada: {@Operation}", ClassName, operation.ClientOperationId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{Class} - Cambios guardados despu�s de insertar operaci�n: {@Operation}", ClassName, operation.ClientOperationId);


        var aux = AuxiliaryInformation.Create(
            operation.ClientOperationId,
            prevalidationResult.Catalogs.Source!.OriginId,
            prevalidationResult.Catalogs.CollectionMethod!.ConfigurationParameterId,
            prevalidationResult.Catalogs.PaymentMethod!.ConfigurationParameterId,
            int.TryParse(command.CollectionAccount, out var acc) ? acc : 0,
            command.PaymentMethodDetail ?? JsonDocument.Parse("{}"),
            tax.CertificationStatusId,
            tax.TaxConditionId,
            tax.WithheldAmount,
            command.VerifiableMedium ?? JsonDocument.Parse("{}"),
            (int)(prevalidationResult.BankId ?? 0),
            DateTime.SpecifyKind(command.DepositDate, DateTimeKind.Utc),
            command.SalesUser,
            prevalidationResult.Catalogs.OriginModality!.ConfigurationParameterId,
            0,
            prevalidationResult.Catalogs.Channel?.ChannelId ?? 0,
            command.User).Value;
        auxiliaryInformationRepository.Insert(aux);

        logger.LogInformation("{Class} - Informaci�n auxiliar creada e insertada: {@Aux}", ClassName, aux.ClientOperationId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{Class} - Cambios guardados despu�s de insertar informaci�n auxiliar: {@Aux}", ClassName, aux.ClientOperationId);


        await operationCompleted.ExecuteAsync(operation, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return (operation, tax);
    }
    
    public async Task ExecuteAsync(
        ClientOperation operation,
        AuxiliaryInformation auxiliaryInformation,
        CancellationToken cancellationToken)
    {
        
        clientOperationRepository.Insert(operation);
        logger.LogInformation("{Class} - Operacion creada e insertada: {@Operation}, fecha: {@fecha}", ClassName, operation.ClientOperationId, operation.ProcessDate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("{Class} - Cambios guardados despues de insertar operacion: {@Operation}, fecha: {@fecha}", ClassName, operation.ClientOperationId, operation.ProcessDate);
        var info = AuxiliaryInformation.Create(
            operation.ClientOperationId,
            auxiliaryInformation.OriginId,
            auxiliaryInformation.CollectionMethodId,
            auxiliaryInformation.PaymentMethodId,
            auxiliaryInformation.CollectionAccount,
            auxiliaryInformation.PaymentMethodDetail,
            auxiliaryInformation.CertificationStatusId,
            auxiliaryInformation.TaxConditionId,
            auxiliaryInformation.ContingentWithholding,
            auxiliaryInformation.VerifiableMedium,
            auxiliaryInformation.CollectionBankId,
            auxiliaryInformation.DepositDate,
            auxiliaryInformation.SalesUser,
            auxiliaryInformation.OriginModalityId,
            auxiliaryInformation.CityId,
            auxiliaryInformation.ChannelId,
            auxiliaryInformation.UserId).Value;

        auxiliaryInformationRepository.Insert(info);

        logger.LogInformation("{Class} - Informacion auxiliar creada e insertada: {@Aux}", ClassName, info.ClientOperationId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{Class} - Cambios guardados despues de insertar informaci�n auxiliar: {@Aux}", ClassName, info.ClientOperationId);
       
    }
} 