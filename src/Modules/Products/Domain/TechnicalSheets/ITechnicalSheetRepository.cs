namespace Products.Domain.TechnicalSheets;

public interface ITechnicalSheetRepository
{
    Task AddAsync(TechnicalSheet technicalSheet, CancellationToken cancellationToken = default);
}
