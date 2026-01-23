namespace Reports.Domain.LoadingInfo.Balances;

public interface IBalanceSheetReadRepository
{
    IAsyncEnumerable<BalanceSheetReadRow> ReadBalancesAsync(
      DateTime closingDateUtc,
        int portfolioId,
        CancellationToken cancellationToken);
}