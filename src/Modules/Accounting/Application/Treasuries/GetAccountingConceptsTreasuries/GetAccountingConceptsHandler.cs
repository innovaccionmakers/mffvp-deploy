using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Accounting.Application.Treasuries.GetAccountingConceptsTreasuries
{
    internal class GetAccountingConceptsHandler(
    ITreasuryRepository treasuryRepository) : IQueryHandler<GetAccountingConceptsTreasuriesQuery, IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse>>> Handle(
            GetAccountingConceptsTreasuriesQuery query,
            CancellationToken cancellationToken)
        { 
            var treasury = await treasuryRepository.GetAccountingConceptsTreasuriesAsync(query.PortfolioIds, cancellationToken);

            var response = treasury
            .Select(t => new GetAccountingConceptsTreasuriesResponse(
                t.PortfolioId,
                t.DebitAccount))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse>>(response);
        }
    }
}
