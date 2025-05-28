namespace Products.Domain.Offices;

public interface IOfficeRepository
{
    Task<IReadOnlyCollection<Office>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Office?> GetAsync(int officeId, CancellationToken cancellationToken = default);
    void Insert(Office office);
    void Update(Office office);
    void Delete(Office office);
}