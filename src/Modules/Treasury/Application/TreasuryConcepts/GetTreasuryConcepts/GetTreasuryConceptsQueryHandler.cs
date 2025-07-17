using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Integrations.TreasuryConcepts.GetTreasuryConcepts;

namespace Treasury.Application.TreasuryConcepts.GetTreasuryConcepts;

public class GetTreasuryConceptsQueryHandler(
    ITreasuryConceptRepository treasuryConceptRepository
) : IQueryHandler<GetTreasuryConceptsQuery, IReadOnlyCollection<TreasuryConcept>>
{
    public async Task<Result<IReadOnlyCollection<TreasuryConcept>>> Handle(
        GetTreasuryConceptsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await treasuryConceptRepository.GetAllAsync(cancellationToken);
        return Result.Success(result);
    }
}