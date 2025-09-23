using Common.SharedKernel.Domain;
using System.ComponentModel;

namespace Customers.Domain.People;

public interface IPersonRepository
{
    Task<IReadOnlyCollection<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person?> GetAsync(long personId, CancellationToken cancellationToken = default);

    Task<Person?> GetForIdentificationAsync(Guid? DocumentType, string Identification,
        CancellationToken cancellationToken = default);
    

    void Insert(Person person);
    void Update(Person person);
    void Delete(Person person);

    Task<Person?> GetByIdentificationAsync(string identification, string documentTypeCode,
        CancellationToken cancellationToken = default);

    Task<bool?> GetExistingHomologatedCode(string homologatedCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PersonInformation>> GetActivePersonsByFilterAsync(string? identificationType,
                                                       SearchByType? searchBy = null,
                                                       string? text = null,
                                                       CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Person>> GetPersonsByDocumentsAsync(
        IReadOnlyCollection<PersonDocumentKey> documents,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PeopleByIdentifications?>> GetPeoplebyIdentificationsAsync(IEnumerable<string> Identification, CancellationToken cancellationToken = default);
}