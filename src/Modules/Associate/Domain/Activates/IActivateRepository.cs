namespace Associate.Domain.Activates;

public interface IActivateRepository
{
    Task<IReadOnlyCollection<Activate>> GetAllAsync(CancellationToken cancellationToken = default);
    bool GetByIdTypeAndNumber(string IdentificationType, string identification);
    void Insert(Activate activate);
    Task<Activate?> GetByIdAsync(long activateId, CancellationToken cancellationToken = default);
}