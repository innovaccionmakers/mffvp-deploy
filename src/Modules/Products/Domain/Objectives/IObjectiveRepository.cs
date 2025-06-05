namespace Products.Domain.Objectives;

public interface IObjectiveRepository
{
    Task<IReadOnlyCollection<Objective>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Objective?> GetAsync(int objectiveId, CancellationToken cancellationToken = default);
    Task<Objective?> GetByIdAsync(int objectiveId, CancellationToken ct);

    Task<IReadOnlyCollection<Objective>> GetByAffiliateAsync(int affiliateId,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(int affiliateId, CancellationToken ct = default);

    Task<bool> AnyWithStatusAsync(int affiliateId, string status, CancellationToken ct = default);

    Task<IReadOnlyCollection<Objective>> GetByAffiliateAsync(
        int affiliateId,
        string? status,
        CancellationToken ct = default);
}