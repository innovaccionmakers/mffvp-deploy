using Closing.Presentation.GraphQL.DTOs;

namespace Closing.Presentation.GraphQL;

public interface IClosingExperienceQueries
{
    Task<ProfitAndLossDto?> GetProfitAndLossAsync(int portfolioId, DateTime effectiveDate, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PortfolioValuationDto>> GetPortfolioValuation(DateOnly closingDate, CancellationToken cancellationToken);
}