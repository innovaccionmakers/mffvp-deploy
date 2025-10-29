namespace Accounting.Domain.ConsecutiveFiles;

public interface IConsecutiveFileRepository
{
    Task<ConsecutiveFile?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<ConsecutiveFile?> GetByGenerationDateAsync(DateTime generationDate, CancellationToken cancellationToken);
    Task AddAsync(ConsecutiveFile consecutiveFile, CancellationToken cancellationToken);
    Task UpdateAsync(ConsecutiveFile consecutiveFile);
}
