namespace Products.Domain.Alternatives;
public interface IAlternativeRepository
{
    Task<IReadOnlyCollection<Alternative>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Alternative?> GetAsync(long alternativeId, CancellationToken cancellationToken = default);
    void Insert(Alternative alternative);
    void Update(Alternative alternative);
    void Delete(Alternative alternative);
}