using Accounting.Domain.AccountingAssistants;
using Accounting.Infrastructure.Database;

namespace Accounting.Infrastructure.AccountingAssistants;

public sealed class AccountingAssistantRepository(AccountingDbContext context) : IAccountingAssistantRepository
{
    public async Task AddRangeAsync(IEnumerable<AccountingAssistant> accountingAssistants, CancellationToken cancellationToken = default)
    {
        await context.AccountingAssistants.AddRangeAsync(accountingAssistants, cancellationToken);
    }
}
