namespace Accounting.Domain.Consecutives;

public interface IConsecutiveRepository
{
    Task<Consecutive?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Consecutive?> GetConsecutiveByNatureAsync(string nature);
    Task<IReadOnlyCollection<Consecutive>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Consecutive consecutive, CancellationToken cancellationToken = default);
    Task UpdateIncomeConsecutiveAsync(int newConsecutiveNumber, CancellationToken cancellationToken = default);
    Task UpdateEgressConsecutiveAsync(int newConsecutiveNumber, CancellationToken cancellationToken = default);
    Task UpdateYieldsConsecutiveAsync(int newConsecutiveNumber, CancellationToken cancellationToken = default);
    Task UpdateConceptConsecutiveAsync(int newConsecutiveNumber, CancellationToken cancellationToken = default);
    Task<bool> IsSourceDocumentInUseAsync(
        string sourceDocument,
        long excludedConsecutiveId,
        CancellationToken cancellationToken = default);
}
