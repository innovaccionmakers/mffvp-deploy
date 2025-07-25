using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.BankAccounts;
using Treasury.Integrations.BankAccounts.GetBankAccountsByPortfolioAndIssuer;

namespace Treasury.Application.BankAccounts.GetBankAccountsByPortfolioAndIssuer;

public class GetBankAccountsByPortfolioAndIssuerQueryHandler(
    IBankAccountRepository bankAccountRepository) : IQueryHandler<GetBankAccountsByPortfolioAndIssuerQuery, IReadOnlyCollection<BankAccount>>
{
    public async Task<Result<IReadOnlyCollection<BankAccount>>> Handle(
        GetBankAccountsByPortfolioAndIssuerQuery request,
        CancellationToken cancellationToken)
    {
        var result = await bankAccountRepository.GetByPortfolioAndIssuerAsync(
            request.PortfolioId,
            request.IssuerId,
            cancellationToken);

        return Result.Success(result);
    }
}