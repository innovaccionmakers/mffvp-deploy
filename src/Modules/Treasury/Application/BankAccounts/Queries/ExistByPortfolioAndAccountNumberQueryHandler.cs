using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.BankAccounts;
using Treasury.Integrations.BankAccounts.Queries;

namespace Treasury.Application.BankAccounts.Queries;

public sealed class ExistByPortfolioAndAccountNumberQueryHandler(IBankAccountRepository bankAccountRepository) : IQueryHandler<ExistByPortfolioAndAccountNumberQuery, bool>
{
    public async Task<Result<bool>> Handle(ExistByPortfolioAndAccountNumberQuery request, CancellationToken cancellationToken)
    {
        var result = await bankAccountRepository.ExistByPortfolioAndAccountNumberAsync(request.PortfolioId, request.AccountNumber, cancellationToken);
        return Result.Success(result);
    }
}
