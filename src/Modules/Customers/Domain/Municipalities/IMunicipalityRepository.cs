namespace Customers.Domain.Municipalities;
public interface IMunicipalityRepository
{
    Task<IReadOnlyCollection<Municipality>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Municipality?> GetAsync(string homologatedCode, CancellationToken cancellationToken = default);
    void Insert(Municipality municipality);
    void Update(Municipality municipality);
    void Delete(Municipality municipality);
}