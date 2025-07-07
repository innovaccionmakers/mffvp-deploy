using System.Text.Json;
using Closing.IntegrationEvents.CreateClientOperationRequested;
using Common.SharedKernel.Application.EventBus;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Contributions.TransactionControl;

public sealed class ContributionTransactionControl(
    IClientOperationRepository clientOperationRepository,
    IAuxiliaryInformationRepository auxiliaryInformationRepository,
    IEventBus eventBus,
    IUnitOfWork unitOfWork,
    ITaxCalculator taxCalculator)
    : IContributionTransactionControl
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<(ClientOperation Operation, TaxResult Tax)> ExecuteAsync(
        CreateContributionCommand command,
        PrevalidationResult prevalidationResult,
        CancellationToken cancellationToken)
    {
        var isCertified = command.CertifiedContribution?.Trim().ToUpperInvariant() == "SI";
        var tax = await taxCalculator.ComputeAsync(
            prevalidationResult.AffiliateFound,
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
            prevalidationResult.Catalogs.Subtype?.SubtransactionTypeId ?? 0,
            DateTime.UtcNow).Value;
        clientOperationRepository.Insert(operation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var aux = AuxiliaryInformation.Create(
            operation.ClientOperationId,
            prevalidationResult.Catalogs.Source!.OriginId,
            prevalidationResult.Catalogs.CollectionMethod!.ConfigurationParameterId,
            prevalidationResult.Catalogs.PaymentMethod!.ConfigurationParameterId,
            int.TryParse(command.CollectionAccount, out var acc) ? acc : 0,
            command.PaymentMethodDetail ?? JsonDocument.Parse("{}"),
            tax.CertificationStatusId,
            tax.TaxConditionId,
            0,
            command.VerifiableMedium ?? JsonDocument.Parse("{}"),
            prevalidationResult.Bank?.BankId ?? 0,
            DateTime.SpecifyKind(command.DepositDate, DateTimeKind.Utc),
            command.SalesUser,
            prevalidationResult.Catalogs.OriginModality!.ConfigurationParameterId,
            0,
            prevalidationResult.Catalogs.Channel?.ChannelId ?? 0,
            int.TryParse(command.User, out var uid) ? uid : 0).Value;
        auxiliaryInformationRepository.Insert(aux);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createClosingEvent = new CreateClientOperationRequestedIntegrationEvent(
            operation.ClientOperationId,
            operation.RegistrationDate,
            operation.AffiliateId,
            operation.ObjectiveId,
            operation.PortfolioId,
            operation.Amount,
            operation.ProcessDate,
            operation.SubtransactionTypeId,
            operation.ApplicationDate);

        await eventBus.PublishAsync(createClosingEvent, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return (operation, tax);
    }
} 