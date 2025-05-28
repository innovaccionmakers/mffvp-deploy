namespace Associate.Domain.Activates;

public interface IActivateRepository
{
    Task<IReadOnlyCollection<Activate>> GetAllAsync(CancellationToken cancellationToken = default);
    Activate GetByIdTypeAndNumber(string IdentificationType, string identification);
    void Insert(Activate activate);    
    void Update(Activate activate);
}