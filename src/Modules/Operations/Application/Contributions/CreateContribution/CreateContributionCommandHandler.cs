using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Application.Abstractions.Services.TrustCreation;
using Common.SharedKernel.Application.Closing;
using Operations.Application.Abstractions.Data;
using Operations.Domain.TemporaryClientOperations;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Integrations.Contributions;
using System.Text.Json;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
    IPrevalidate prevalidator,
    ITransactionControl transactionControl,
    ITrustCreation trustCreation,
    IClosingExecutionStore closingStore,
    ITemporaryClientOperationRepository tempClientOpRepository,
    ITemporaryAuxiliaryInformationRepository tempAuxRepository,
    ITaxCalculator taxCalculator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateContributionCommand, ContributionResponse>
{
    public async Task<Result<ContributionResponse>> Handle(CreateContributionCommand command,
        CancellationToken cancellationToken)
    {
        var prevalidationResult = await prevalidator.ValidateAsync(command, cancellationToken);

        if (!prevalidationResult.IsSuccess)
            return Result.Failure<ContributionResponse>(prevalidationResult.Error!);
        var portfolioId = prevalidationResult.Value.RemoteData.PortfolioId;
        if (await closingStore.IsClosingActiveAsync(portfolioId, cancellationToken))
        {
            var isCertified = command.CertifiedContribution?.Trim().ToUpperInvariant() == "SI";
            var tax = await taxCalculator.ComputeAsync(
                prevalidationResult.Value.AffiliateFound,
                isCertified,
                command.Amount,
                cancellationToken);

            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var tempOp = TemporaryClientOperation.Create(
                DateTime.UtcNow,
                prevalidationResult.Value.RemoteData.AffiliateId,
                prevalidationResult.Value.RemoteData.ObjectiveId,
                portfolioId,
                command.Amount,
                DateTime.SpecifyKind(command.ExecutionDate, DateTimeKind.Utc),
                prevalidationResult.Value.Catalogs.Subtype?.SubtransactionTypeId ?? 0,
                DateTime.UtcNow).Value;

            tempClientOpRepository.Insert(tempOp);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var tempAux = TemporaryAuxiliaryInformation.Create(
                tempOp.TemporaryClientOperationId,
                prevalidationResult.Value.Catalogs.Source!.OriginId,
                prevalidationResult.Value.Catalogs.CollectionMethod!.ConfigurationParameterId,
                prevalidationResult.Value.Catalogs.PaymentMethod!.ConfigurationParameterId,
                int.TryParse(command.CollectionAccount, out var acc) ? acc : 0,
                command.PaymentMethodDetail ?? JsonDocument.Parse("{}"),
                tax.CertificationStatusId,
                tax.TaxConditionId,
                0,
                command.VerifiableMedium ?? JsonDocument.Parse("{}"),
                prevalidationResult.Value.Bank?.BankId ?? 0,
                DateTime.SpecifyKind(command.DepositDate, DateTimeKind.Utc),
                command.SalesUser,
                prevalidationResult.Value.Catalogs.OriginModality!.ConfigurationParameterId,
                0,
                prevalidationResult.Value.Catalogs.Channel?.ChannelId ?? 0,
                int.TryParse(command.User, out var uid) ? uid : 0).Value;

            tempAuxRepository.Insert(tempAux);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var queueResp = new ContributionResponse(
                null,
                portfolioId.ToString(),
                prevalidationResult.Value.RemoteData.PortfolioName,
                tax.TaxConditionName,
                tax.WithheldAmount,
                true);

            return Result.Success(queueResp, "Contribución en cola durante cierre");
        }

        var (operation, taxResult) = await transactionControl.ExecuteAsync(command, prevalidationResult.Value, cancellationToken);

        await trustCreation.ExecuteAsync(command, operation, taxResult, cancellationToken);

        var resp = new ContributionResponse(
            operation.ClientOperationId,
            operation.PortfolioId.ToString(),
            prevalidationResult.Value.RemoteData.PortfolioName,
            taxResult.TaxConditionName,
            taxResult.WithheldAmount);

        return Result.Success(resp, "Transacción causada Exitosamente");
    }}