using Accounting.Domain.AccountingInconsistencies;

namespace Accounting.Application.AccountingValidator.Reports;

public class AccountingInconsistenciesRequest
{
    public DateTime ProcessDate { get; set; }
    public IEnumerable<AccountingInconsistency> Inconsistencies { get; set;}
}
