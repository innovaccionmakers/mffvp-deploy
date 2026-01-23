namespace Reports.Domain.LoadingInfo.Closing;

public interface IClosingSheetReadRepository
{
    IAsyncEnumerable<ClosingSheetReadRow> ReadClosingAsync(
    DateTime closingDateUtc,
    int portfolioId,
    CancellationToken cancellationToken);
}
