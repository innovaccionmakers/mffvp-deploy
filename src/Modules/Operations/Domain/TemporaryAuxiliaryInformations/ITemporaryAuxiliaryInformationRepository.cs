namespace Operations.Domain.TemporaryAuxiliaryInformations;

public interface ITemporaryAuxiliaryInformationRepository
{
    Task<IReadOnlyCollection<TemporaryAuxiliaryInformation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TemporaryAuxiliaryInformation?> GetAsync(long temporaryAuxiliaryInformationId, CancellationToken cancellationToken = default);
    void Insert(TemporaryAuxiliaryInformation temporaryAuxiliaryInformation);
    void Update(TemporaryAuxiliaryInformation temporaryAuxiliaryInformation);
    void Delete(TemporaryAuxiliaryInformation temporaryAuxiliaryInformation);
    void DeleteRange(IEnumerable<TemporaryAuxiliaryInformation> infos);
    Task<IReadOnlyCollection<TemporaryAuxiliaryInformation>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
    Task<int> DeleteByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
}
