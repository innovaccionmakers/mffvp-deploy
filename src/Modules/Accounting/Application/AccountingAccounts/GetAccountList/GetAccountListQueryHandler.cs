using Accounting.Domain.AccountingAccounts;
using Accounting.Integrations.AccountingAccount;
using Accounting.Integrations.AccountingAccount.GetAccountList;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Accounting.Application.AccountingAccounts.GetAccountList
{
    internal class GetAccountListQueryHandler(
        IAccountingAccountRepository accountingAccountRepository) : IQueryHandler<GetAccountListQuery, IReadOnlyCollection<AccountingAccountResponse>>
    {
        public async Task<Result<IReadOnlyCollection<AccountingAccountResponse>>> Handle(GetAccountListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var accountingAccounts = await accountingAccountRepository.GetAccountListAsync(cancellationToken);

                var response = accountingAccounts.Select(x => new AccountingAccountResponse(x.Account, x.Name)).ToList();

                return Result.Success<IReadOnlyCollection<AccountingAccountResponse>>(response);
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
        }
    }
}
