using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.GetTreasuriesByPortfolioIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Accounting.Application.Treasuries.GetTreasuriesByPortfolioIds
{
    internal class GetTreasuriesByPortfolioIdsHandler(
    ITreasuryRepository treasuryRepository) : IQueryHandler<GetTreasuriesByPortfolioIdsQuery, IReadOnlyCollection<GetTreasuriesByPortfolioIdsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetTreasuriesByPortfolioIdsResponse>>> Handle(
            GetTreasuriesByPortfolioIdsQuery query,
            CancellationToken cancellationToken)
        { 
            var treasury = await treasuryRepository.GetTreasuriesByPortfolioIdsAsync(query.PortfolioIds, cancellationToken);

            var response = treasury
            .Select(t => new GetTreasuriesByPortfolioIdsResponse(
                t.PortfolioId,
                t.DebitAccount))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetTreasuriesByPortfolioIdsResponse>>(response);
        }
    }
}
