namespace People.Domain.People;

public interface IPersonRepository
{
    Task<IReadOnlyCollection<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person?> GetAsync(long personId, CancellationToken cancellationToken = default);
    void Insert(Person person);
    void Update(Person person);
    void Delete(Person person);
}