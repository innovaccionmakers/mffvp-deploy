using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Treasury.Domain.BankAccounts;
using Treasury.Domain.Issuers;
using Treasury.Integrations.BankAccounts.GetBankAccountsByPortfolio;

namespace Treasury.Application.BankAccounts.GetBankAccountsByPortfolio;

public class GetBankAccountsByPortfolioQueryHandler(
    IBankAccountRepository bankAccountRepository) : IQueryHandler<GetBankAccountsByPortfolioQuery, IReadOnlyCollection<BankAccount>>
{
    public async Task<Result<IReadOnlyCollection<BankAccount>>> Handle(
        GetBankAccountsByPortfolioQuery request, 
        CancellationToken cancellationToken)
    {
        var result = await bankAccountRepository.GetByPortfolioIdAsync(request.PortfolioId, cancellationToken);

        return Result.Success(result);
    }
}