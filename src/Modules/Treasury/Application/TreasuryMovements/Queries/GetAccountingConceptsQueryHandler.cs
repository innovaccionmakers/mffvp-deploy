using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.TreasuryMovements;
using Treasury.Integrations.TreasuryMovements.Queries;

namespace Treasury.Application.TreasuryMovements.Queries
{
    internal class GetAccountingConceptsQueryHandler(
        ITreasuryMovementRepository treasuryMovementRepository) : IQueryHandler<GetAccountingConceptsQuery, IReadOnlyCollection<TreasuryMovement>>
    {
        public async Task<Result<IReadOnlyCollection<TreasuryMovement>>> Handle(GetAccountingConceptsQuery request, CancellationToken cancellationToken)
        {
            var result = await treasuryMovementRepository.GetAccountingConceptsAsync(request.PortfolioIds, request.ProcessDate, cancellationToken);
            return Result.Success(result);
        }
    }
}
