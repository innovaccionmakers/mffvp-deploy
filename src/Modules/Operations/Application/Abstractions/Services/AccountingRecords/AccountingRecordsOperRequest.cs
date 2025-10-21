namespace Operations.Application.Abstractions.Services.AccountingRecords;

public sealed record AccountingRecordsOperRequest(
    decimal Amount,
    int CauseId,
    int AffiliateId,
    int ObjectiveId);
