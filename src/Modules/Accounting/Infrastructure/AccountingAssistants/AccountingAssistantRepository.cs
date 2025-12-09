using Accounting.Domain.AccountingAssistants;
using Accounting.Infrastructure.Database;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.AccountingAssistants;

public sealed class AccountingAssistantRepository(AccountingDbContext context) : IAccountingAssistantRepository
{
    public async Task AddRangeAsync(IEnumerable<AccountingAssistant> accountingAssistants, CancellationToken cancellationToken = default)
    {
        var assistantsList = accountingAssistants.ToList();

        if (assistantsList.Count == 0)
            return;

        var bulkConfig = new BulkConfig
        {
            BatchSize = 4000,
            BulkCopyTimeout = 300,
        };

        await context.BulkInsertAsync(assistantsList, bulkConfig, cancellationToken: cancellationToken);
    }

    public async Task DeleteRangeAsync(CancellationToken cancellationToken = default)
    {
        await context.AccountingAssistants.ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AccountingAssistant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
       return await context.AccountingAssistants.ToListAsync(cancellationToken);
    }
}
