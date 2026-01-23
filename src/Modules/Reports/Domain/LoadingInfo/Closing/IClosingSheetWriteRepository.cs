namespace Reports.Domain.LoadingInfo.Closing;

public interface IClosingSheetWriteRepository
{
    Task DeleteByClosingDateAndPortfolioAsync(DateTime closingDateUtc, int portfolioId, CancellationToken cancellationToken);

    Task BulkInsertAsync(IReadOnlyList<ClosingSheet> rows, CancellationToken cancellationToken);
}
