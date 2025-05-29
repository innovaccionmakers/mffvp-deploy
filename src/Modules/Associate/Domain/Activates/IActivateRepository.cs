namespace Associate.Domain.Activates;

public interface IActivateRepository
{
    Task<IReadOnlyCollection<Activate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Activate> GetByIdTypeAndNumber(string IdentificationType, string identification, CancellationToken cancellationToken = default);
    void Insert(Activate activate, CancellationToken cancellationToken = default);    
    void Update(Activate activate, CancellationToken cancellationToken = default);
    Task<Activate?> GetByIdAsync(long activateId, CancellationToken cancellationToken = default);
}