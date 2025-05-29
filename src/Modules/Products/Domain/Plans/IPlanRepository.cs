namespace Products.Domain.Plans;

public interface IPlanRepository
{
    Task<IReadOnlyCollection<Plan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Plan?> GetAsync(int planId, CancellationToken cancellationToken = default);
    void Insert(Plan plan);
    void Update(Plan plan);
    void Delete(Plan plan);
}