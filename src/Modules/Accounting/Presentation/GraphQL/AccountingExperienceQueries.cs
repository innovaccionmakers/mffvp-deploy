using Accounting.Integrations.AccountingAccount.GetAccountList;
using Accounting.Presentation.DTOs;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class AccountingExperienceQueries(
        ISender mediator) : IAccountingExperienceQueries
    {
        public async Task<IReadOnlyCollection<AccountingAccountsDto>> GetAccountListAsync(CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetAccountListQuery(), cancellationToken);

            if (!result.IsSuccess || result.Value == null)
                throw new InvalidOperationException("Failed to retrieve transaction types.");

            var accountList = result.Value;

            return accountList.Select(x => new AccountingAccountsDto(
                x.Account,
                x.Name
            )).ToList();
        }
    }
}
