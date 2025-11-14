
using Closing.Application.Abstractions.External;
using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Domain.ClientOperations;
using Closing.Domain.PortfolioValuations;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;

namespace Closing.Application.Closing.Services.Validation;

public sealed class ClosingBusinessRules(
    IPortfolioValidator portfolioValidator,
    IPortfolioValuationRepository portfolioValuationRepository,
    IOperationTypesLocator operationTypeIdProvider,
    IClientOperationRepository clientOperationRepository)
    : IClosingBusinessRules
{
    public async Task<Result<bool>> IsFirstClosingDayAsync(int portfolioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var portfolioDataResult = await portfolioValidator.GetPortfolioDataAsync(portfolioId, cancellationToken);
        if (portfolioDataResult.IsFailure)
            return Result.Failure<bool>(portfolioDataResult.Error);

        var currentDateUtc = portfolioDataResult.Value.CurrentDate;

        var exists = await portfolioValuationRepository
            .ExistsByPortfolioAndDateAsync(portfolioId, currentDateUtc, cancellationToken);

        return Result.Success(!exists);
    }

    public async Task<Result<bool>> HasDebitNotesAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var debitNoteIdResult = await operationTypeIdProvider.GetOperationTypeByNameAsync(OperationTypeAttributes.Names.DebitNote, cancellationToken);
        if (debitNoteIdResult.IsFailure)
            return Result.Failure<bool>(debitNoteIdResult.Error!);

        var debitNoteOperationTypeId = debitNoteIdResult.Value.OperationTypeId;

        var exists = await clientOperationRepository
            .ClientOperationsExistsAsync(portfolioId, closingDateUtc, debitNoteOperationTypeId, cancellationToken);

        return Result.Success(exists);
    }
}
