using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingOperations
{
    public sealed record class AccountingOperationsCommand(
        IEnumerable<int> PortfolioIds,
        DateTime ProcessDate
        ) : ICommand<string>;
}
