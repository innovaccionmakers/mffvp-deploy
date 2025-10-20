using Common.SharedKernel.Domain;
namespace Operations.Application.Abstractions.Services.AccountingRecords;

public interface IAccountingRecordsOper
{
    Task<Result<AccountingRecordsOperResult>> ExecuteAsync(
        AccountingRecordsOperRequest request,
        AccountingRecordsValidationResult validationResult,
        CancellationToken cancellationToken);
}
