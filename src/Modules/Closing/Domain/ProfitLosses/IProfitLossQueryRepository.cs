
namespace Closing.Domain.ProfitLosses;

// SOLO lecturas (para Simulación/Cierre).
public interface IProfitLossQueryRepository
{
    Task<IReadOnlyList<ProfitLossConceptSummary>> GetReadOnlyConceptSummaryAsync(
        int portfolioId,
        DateTime effectiveDateUtc,
        CancellationToken cancellationToken = default);

    Task<bool> PandLExistsAsync(
        int portfolioId,
        DateTime effectiveDateUtc,
        CancellationToken cancellationToken = default);
}