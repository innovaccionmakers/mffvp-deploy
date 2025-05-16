namespace Products.Domain.Objectives;
public interface IObjectiveRepository
{
    Task<IReadOnlyCollection<Objective>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Objective?> GetAsync(long objectiveId, CancellationToken cancellationToken = default);
    void Insert(Objective objective);
    void Update(Objective objective);
    void Delete(Objective objective);
}