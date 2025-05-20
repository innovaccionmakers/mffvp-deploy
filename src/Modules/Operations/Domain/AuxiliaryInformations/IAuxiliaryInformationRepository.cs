namespace Operations.Domain.AuxiliaryInformations;

public interface IAuxiliaryInformationRepository
{
    Task<IReadOnlyCollection<AuxiliaryInformation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AuxiliaryInformation?> GetAsync(long auxiliaryinformationId, CancellationToken cancellationToken = default);
    void Insert(AuxiliaryInformation auxiliaryinformation);
    void Update(AuxiliaryInformation auxiliaryinformation);
    void Delete(AuxiliaryInformation auxiliaryinformation);
}