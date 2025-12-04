using System;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.AccountingRecords;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Domain.ClientOperations;
using Operations.Domain.TrustOperations;

namespace Operations.Application.AccountingRecords.Services;

public sealed class AccountingRecordsOper(
    IUnitOfWork unitOfWork,
    IClientOperationRepository clientOperationRepository,
    IOperationCompleted operationCompleted,
    ITrustUpdater trustUpdater,
    IPortfolioValuationProvider portfolioValuationProvider,
    ITrustOperationRepository trustOperationRepository,
    ITrustDetailsProvider trustDetailsProvider)
    : IAccountingRecordsOper
{
    private const string SuccessTemplate = "La nota débito se creó correctamente. ID de la operación: {0}.";

    public async Task<Result<AccountingRecordsOperResult>> ExecuteAsync(
        AccountingRecordsOperRequest request,
        AccountingRecordsValidationResult validationResult,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var original = validationResult.OriginalOperation;
        var valuationDate = validationResult.PortfolioCurrentDate;

        var valuationResult = await portfolioValuationProvider
            .GetUnitValueAsync(original.PortfolioId, valuationDate, cancellationToken);

        if (valuationResult.IsFailure)
        {
            return Result.Failure<AccountingRecordsOperResult>(valuationResult.Error);
        }

        var unitValue = valuationResult.Value;

        var registrationDate = DateTime.UtcNow;
        var applicationDate = DateTime.UtcNow;
        var processDate = DateTime.SpecifyKind(
            validationResult.PortfolioCurrentDate.AddDays(1),
            DateTimeKind.Utc);

        var units = decimal.Round(
            request.Amount / unitValue,
            16,
            MidpointRounding.AwayFromZero);

        var debitNote = ClientOperation.Create(
            registrationDate,
            request.AffiliateId,
            request.ObjectiveId,
            original.PortfolioId,
            request.Amount,
            processDate,
            validationResult.DebitNoteOperationTypeId,
            applicationDate,
            LifecycleStatus.Active,
            request.CauseId,
            validationResult.TrustId,
            original.ClientOperationId,
            units).Value;

        clientOperationRepository.Insert(debitNote);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        original.UpdateDetails(
            original.RegistrationDate,
            original.AffiliateId,
            original.ObjectiveId,
            original.PortfolioId,
            original.Amount,
            original.ProcessDate,
            original.OperationTypeId,
            original.ApplicationDate,
            LifecycleStatus.AnnulledByDebitNote,
        request.CauseId,
            original.TrustId,
            original.LinkedClientOperationId,
            original.Units);

        clientOperationRepository.Update(original);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await operationCompleted.ExecuteAsync(original, cancellationToken);

        var trustDetailsResult = await trustDetailsProvider
            .GetAsync(validationResult.TrustId, cancellationToken);

        if (trustDetailsResult.IsFailure)
        {
            return Result.Failure<AccountingRecordsOperResult>(trustDetailsResult.Error);
        }

        var trustOperationTimestamp = DateTime.UtcNow;

        var trustOperationUnits = decimal.Round(
            Math.Abs(trustDetailsResult.Value.Earnings) / unitValue,
            16,
            MidpointRounding.AwayFromZero);

        var trustOperationResult = TrustOperation.Create(
            debitNote.ClientOperationId,
            validationResult.TrustId,
            -trustDetailsResult.Value.Earnings,
            trustOperationUnits,
            validationResult.TrustAdjustmentOperationTypeId,
            original.PortfolioId,
            trustOperationTimestamp,
            processDate,
            trustOperationTimestamp);

        if (trustOperationResult.IsFailure)
        {
            return Result.Failure<AccountingRecordsOperResult>(trustOperationResult.Error);
        }

        await trustOperationRepository.AddAsync(trustOperationResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var update = new TrustUpdate(
            original.ClientOperationId,
            LifecycleStatus.AnnulledByDebitNote,
            0m,
            0m,
            0m,
            0m,
            0m,
            0m,
            0m,
            0m,
            0m,
            processDate);

        var trustUpdateResult = await trustUpdater
            .UpdateAsync(update, cancellationToken);

        if (trustUpdateResult.IsFailure)
        {
            return Result.Failure<AccountingRecordsOperResult>(trustUpdateResult.Error);
        }

        await operationCompleted.ExecuteAsync(debitNote, cancellationToken);

        var message = string.Format(SuccessTemplate, debitNote.ClientOperationId);
        return Result.Success(new AccountingRecordsOperResult(debitNote.ClientOperationId, message));
    }
}
