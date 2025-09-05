namespace Reports.Domain.TechnicalSheet;

public interface ITechnicalSheetRepository
{
    Task<IEnumerable<TechnicalSheet>> GetByDateRangeAndPortfolioAsync(DateOnly startDate, DateOnly endDate, int portfolioId, CancellationToken cancellationToken = default);
}
