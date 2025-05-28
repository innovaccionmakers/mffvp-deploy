namespace Products.Domain.Commercials;

public interface ICommercialRepository
{
    Task<IReadOnlyCollection<Commercial>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Commercial?> GetAsync(int commercialId, CancellationToken cancellationToken = default);
    void Insert(Commercial commercial);
    void Update(Commercial commercial);
    void Delete(Commercial commercial);
}