using Treasury.Presentation.DTOs;

namespace Treasury.Presentation.GraphQL;

public interface ITreasuryExperienceQueries
{
    Task<IReadOnlyCollection<IssuerDto>> GetIssuersAsync(CancellationToken cancellationToken = default);
}