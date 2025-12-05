using Accounting.Presentation.DTOs;
using Common.SharedKernel.Presentation.Results;

namespace Accounting.Presentation.GraphQL
{
    public interface IConfigurationGeneralsExperienceQueries
    {
        Task<GraphqlResult<ConfigurationGeneralDto>> GetConfigurationGeneralsAsync(int portfolioId, CancellationToken cancellationToken = default);
    }
}
