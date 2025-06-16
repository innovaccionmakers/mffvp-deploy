namespace Customers.Domain.Countries;
public interface ICountryRepository
{
    Task<IReadOnlyCollection<Country>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Country?> GetAsync(string HomologatedCode, CancellationToken cancellationToken = default);
    void Insert(Country country);
    void Update(Country country);
    void Delete(Country country);
}