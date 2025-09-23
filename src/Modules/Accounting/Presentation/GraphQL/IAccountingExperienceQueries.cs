namespace Accounting.Presentation.GraphQL;

public interface IAccountingExperienceQueries
{
    Task<string> GetAccountingFeesAsync(List<int> portfolioIds, DateTime closingDate, CancellationToken cancellationToken = default);
}
