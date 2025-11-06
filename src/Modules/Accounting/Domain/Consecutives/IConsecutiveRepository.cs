namespace Accounting.Domain.Consecutives;

public interface IConsecutiveRepository
{
    Task<Consecutive?> GetConsecutiveByNatureAsync(string nature);
    Task<IReadOnlyCollection<Consecutive>> GetAllAsync(CancellationToken cancellationToken = default);
}
