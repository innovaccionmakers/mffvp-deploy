namespace Accounting.Domain.AccountingAssistants;

public interface IAccountingAssistantRepository
{
    Task AddRangeAsync(IEnumerable<AccountingAssistant> accountingAssistants, CancellationToken cancellationToken = default);
}
