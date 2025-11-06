using Accounting.Domain.ConsecutiveFiles;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Accounting.Infrastructure.ConsecutiveFiles;

public class ConsecutiveFileRepository(AccountingDbContext context) : IConsecutiveFileRepository
{
    private const int Consecutive = 1;

    public async Task AddAsync(ConsecutiveFile consecutiveFile, CancellationToken cancellationToken)
    {
        await context.ConsecutiveFiles.AddAsync(consecutiveFile, cancellationToken);
    }

    public async Task<ConsecutiveFile?> GetByGenerationDateAsync(DateTime generationDate, CancellationToken cancellationToken)
    {
        return await context.ConsecutiveFiles.FirstOrDefaultAsync(x => x.GenerationDate == generationDate, cancellationToken);

    }

    public async Task<ConsecutiveFile?> GetByCurrentDateAsync(DateTime currentDate, CancellationToken cancellationToken)
    {
        return await context.ConsecutiveFiles
            .Where(x => x.CurrentDate.Date == currentDate.Date)
            .OrderByDescending(x => x.CurrentDate)
            .FirstOrDefaultAsync(cancellationToken);
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

    public async Task DeleteAllAsync(CancellationToken cancellationToken)
    {
        await context.ConsecutiveFiles.ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<(ConsecutiveFile consecutiveFile, int consecutiveNumber)> GetOrCreateNextConsecutiveForTodayAsync(
        DateTime generationDate,
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var todayDate = today.Date;

        var existingRecord = await GetByCurrentDateAsync(todayDate, cancellationToken);

        if (existingRecord is null)
            await DeleteAllAsync(cancellationToken);

        var consecutiveFile = await GetByGenerationDateAsync(generationDate.Date, cancellationToken);
        int consecutive = Consecutive;

        if (consecutiveFile is not null)
        {
            consecutiveFile.Increment();
            consecutive = consecutiveFile.Consecutive;
            await UpdateAsync(consecutiveFile);
        }
        else
        {
            var newConsecutive = ConsecutiveFile.Create(generationDate, 1, today);
            if (newConsecutive.IsFailure)
                throw new Exception($"Error al crear el consecutivo para la fecha {generationDate:yyyy-MM-dd}");

            consecutive = newConsecutive.Value.Consecutive;
            consecutiveFile = newConsecutive.Value;
            await AddAsync(consecutiveFile, cancellationToken);
        }

        return (consecutiveFile, consecutive);
    }
}
