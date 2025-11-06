using Common.SharedKernel.Domain;

namespace Accounting.Domain.AccountingAccounts
{
    public class AccountingAccount : Entity
    {
        public int  AccountingAccountId { get; private set; }
        public string Account { get; private set; }
        public string Name { get; private set; }

        private AccountingAccount() { }

        public static Result<AccountingAccount> Create(
        string account,
        string name)
            {
                var accountingAccount = new AccountingAccount
                {
                    AccountingAccountId = default,
                    Account = account,
                    Name = name
                };
                return Result.Success(accountingAccount);
            }

            public void UpdateDetails(
                string account,
                string name)
            {
                Account = account;
                Name = name;
            }
    }
}
