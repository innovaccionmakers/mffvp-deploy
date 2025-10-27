using Operations.Domain.ClientOperations;

namespace Operations.Application.Abstractions.Services.AccountingRecords;

public sealed record AccountingRecordsValidationResult(
    ClientOperation OriginalOperation,
    long DebitNoteOperationTypeId,
    long TrustAdjustmentOperationTypeId,
    DateTime PortfolioCurrentDate,
    long TrustId,
    int CauseConfigurationParameterId);
