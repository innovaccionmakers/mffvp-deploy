using Accounting.Domain.ConsecutiveFiles;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.ConsecutiveFiles;

public class ConsecutiveFileRepository(AccountingDbContext context) : IConsecutiveFileRepository
{
    public async Task AddAsync(ConsecutiveFile consecutiveFile, CancellationToken cancellationToken)
    {
        await context.ConsecutiveFiles.AddAsync(consecutiveFile, cancellationToken);
    }

    public async Task<ConsecutiveFile?> GetByGenerationDateAsync(DateTime generationDate, CancellationToken cancellationToken)
    {
        return await context.ConsecutiveFiles.FirstOrDefaultAsync(x => x.GenerationDate == generationDate, cancellationToken);

    }

    public async Task<ConsecutiveFile?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await context.ConsecutiveFiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(ConsecutiveFile consecutiveFile)
    {
        context.ConsecutiveFiles.Update(consecutiveFile);
        await Task.CompletedTask;
    }
}
