using System.Text.Json;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Abstractions.Services.QueueTransactions;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Contributions.Services.QueueTransactions;

public sealed class QueueTransactions(
    ITemporaryClientOperationRepository tempClientOpRepository,
    ITemporaryAuxiliaryInformationRepository tempAuxRepository,
    ITaxCalculator taxCalculator,
    IUnitOfWork unitOfWork) : IQueueTransactions
{
    public async Task<Result<ContributionResponse>> ExecuteAsync(
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

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var tempOp = TemporaryClientOperation.Create(
            DateTime.UtcNow,
            prevalidationResult.RemoteData.AffiliateId,
            prevalidationResult.RemoteData.ObjectiveId,
            prevalidationResult.RemoteData.PortfolioId,
            command.Amount,
            DateTime.SpecifyKind(command.ExecutionDate, DateTimeKind.Utc),
            prevalidationResult.Catalogs.Subtype?.OperationTypeId ?? 0,
            DateTime.UtcNow).Value;

        tempClientOpRepository.Insert(tempOp);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tempAux = TemporaryAuxiliaryInformation.Create(
            tempOp.TemporaryClientOperationId,
            prevalidationResult.Catalogs.Source!.OriginId,
            prevalidationResult.Catalogs.CollectionMethod!.ConfigurationParameterId,
            prevalidationResult.Catalogs.PaymentMethod!.ConfigurationParameterId,
            int.TryParse(command.CollectionAccount, out var acc) ? acc : 0,
            command.PaymentMethodDetail ?? JsonDocument.Parse("{}"),
            tax.CertificationStatusId,
            tax.TaxConditionId,
            tax.WithheldAmount,
            command.VerifiableMedium ?? JsonDocument.Parse("{}"),
            prevalidationResult.Bank?.BankId ?? 0,
            DateTime.SpecifyKind(command.DepositDate, DateTimeKind.Utc),
            command.SalesUser,
            prevalidationResult.Catalogs.OriginModality!.ConfigurationParameterId,
            0,
            prevalidationResult.Catalogs.Channel?.ChannelId ?? 0,
            command.User).Value;

        tempAuxRepository.Insert(tempAux);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var response = new ContributionResponse(
            null,
            prevalidationResult.RemoteData.PortfolioId.ToString(),
            prevalidationResult.RemoteData.PortfolioName,
            tax.TaxConditionName,
            tax.WithheldAmount);

        return Result.Success(
            response,
            "La solicitud de aporte fue recibida. El portafolio se encuentra actualmente en proceso de cierre, y una vez finalizado, su solicitud será ejecutada con la fecha del día hábil siguiente.");
    }
}
