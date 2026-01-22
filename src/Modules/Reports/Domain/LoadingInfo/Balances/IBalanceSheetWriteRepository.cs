namespace Reports.Domain.LoadingInfo.Balances;

public interface IBalanceSheetWriteRepository
{
    Task DeleteByClosingDateAndPortfolioAsync(
        DateTime closingDateUtc,
        int portfolioId,
        CancellationToken cancellationToken);

    Task BulkInsertAsync(
        IReadOnlyList<BalanceSheet> rows,
        CancellationToken cancellationToken);
}
