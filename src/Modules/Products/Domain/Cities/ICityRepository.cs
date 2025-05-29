namespace Products.Domain.Cities;

public interface ICityRepository
{
    Task<IReadOnlyCollection<City>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<City?> GetAsync(int cityId, CancellationToken cancellationToken = default);
    void Insert(City city);
    void Update(City city);
    void Delete(City city);
}