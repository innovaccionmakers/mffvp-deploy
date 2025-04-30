namespace Contributions.Domain.ClientOperations;
public interface IClientOperationRepository
{
    Task<IReadOnlyCollection<ClientOperation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientOperation?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(ClientOperation clientoperation);
    void Update(ClientOperation clientoperation);
    void Delete(ClientOperation clientoperation);
}