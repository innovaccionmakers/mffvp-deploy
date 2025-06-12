namespace Customers.Domain.People;

public interface IPersonRepository
{
    Task<IReadOnlyCollection<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person?> GetAsync(long personId, CancellationToken cancellationToken = default);

    Task<Person?> GetForIdentificationAsync(Guid DocumentType, string Identification,
        CancellationToken cancellationToken = default);

    void Insert(Person person);
    void Update(Person person);
    void Delete(Person person);

    Task<Person?> GetByIdentificationAsync(string identification, string documentTypeCode,
        CancellationToken cancellationToken = default);
}