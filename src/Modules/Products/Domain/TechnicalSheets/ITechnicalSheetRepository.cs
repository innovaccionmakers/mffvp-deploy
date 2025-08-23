namespace Products.Domain.TechnicalSheets;

public interface ITechnicalSheetRepository
{
    Task AddAsync(TechnicalSheet technicalSheet, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TechnicalSheet> technicalSheets, CancellationToken cancellationToken = default);

    Task<IEnumerable<TechnicalSheet>> GetByDateRangeAndPortfolioAsync(
        DateOnly startDate,
        DateOnly endDate,
        int portfolioId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByDateAsync(DateTime closingDate, CancellationToken cancellationToken = default);

    Task DeleteByDateAsync(DateTime closingDate, CancellationToken cancellationToken = default);
}
