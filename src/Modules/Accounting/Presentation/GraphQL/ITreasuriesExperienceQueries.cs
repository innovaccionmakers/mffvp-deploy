using Accounting.Presentation.DTOs;
using Accounting.Presentation.GraphQL.Inputs.TreasuriesInput;
using Common.SharedKernel.Presentation.Results;

namespace Accounting.Presentation.GraphQL
{
    public interface ITreasuriesExperienceQueries
    {
        Task<GraphqlResult<TreasuryDto>> GetTreasuriesAsync(GetTreasuryInput input, CancellationToken cancellationToken = default);
    }
}
