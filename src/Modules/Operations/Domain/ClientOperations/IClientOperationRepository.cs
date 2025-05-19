namespace Operations.Domain.ClientOperations;

public interface IClientOperationRepository
{
    Task<IReadOnlyCollection<ClientOperation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientOperation?> GetAsync(long clientoperationId, CancellationToken cancellationToken = default);
    void Insert(ClientOperation clientoperation);
    void Update(ClientOperation clientoperation);
    void Delete(ClientOperation clientoperation);
}