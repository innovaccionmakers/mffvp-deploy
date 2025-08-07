
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.TreasuryMovements;
using Treasury.Integrations.TreasuryMovements.Queries;

namespace Treasury.Application.TreasuryMovements.Queries;

internal sealed class GetTreasuryMovementsByPortfolioIdsQueryHandler(ITreasuryMovementRepository treasuryMovementRepository) : IQueryHandler<GetTreasuryMovementsByPortfolioIdsQuery, IReadOnlyCollection<TreasuryMovement>>
{
    public async Task<Result<IReadOnlyCollection<TreasuryMovement>>> Handle(GetTreasuryMovementsByPortfolioIdsQuery request, CancellationToken cancellationToken)
    {
       var result = await treasuryMovementRepository.GetTreasuryMovementsByPortfolioIdsAsync(request.PortfolioIds, cancellationToken);
       return Result.Success(result);
    }
}
