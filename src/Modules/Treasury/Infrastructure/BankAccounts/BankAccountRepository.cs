using Microsoft.EntityFrameworkCore;
using Treasury.Domain.BankAccounts;
using Treasury.Infrastructure.Database;
using Treasury.Presentation.DTOs;

namespace Treasury.Infrastructure.BankAccounts;

public class BankAccountRepository(TreasuryDbContext context) : IBankAccountRepository
{
    public async Task<BankAccount?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<BankAccount>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.BankAccounts.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(BankAccount bankAccount, CancellationToken cancellationToken = default)
    {
        await context.BankAccounts.AddAsync(bankAccount, cancellationToken);
    }

    public async Task<bool> ExistsAsync(long issuerId, string accountNumber, string accountType, CancellationToken cancellationToken = default)
    {
        return await context.BankAccounts.AnyAsync(
        x => x.IssuerId == issuerId &&
             x.AccountNumber == accountNumber &&
             x.AccountType == accountType,
        cancellationToken);
    }

    public async Task<IReadOnlyCollection<BankAccount>> GetByPortfolioIdAsync(long portfolioId, CancellationToken cancellationToken = default)
    {
        return await context.BankAccounts
            .Include(x => x.Issuer)
            .Where(x => x.PortfolioId == portfolioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BankAccount>> GetByPortfolioAndIssuerAsync(long portfolioId, long issuerId, CancellationToken cancellationToken = default)
    {
        return await context.BankAccounts
            .Include(x => x.Issuer)
            .Where(x => x.PortfolioId == portfolioId && x.IssuerId == issuerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistByPortfolioAndAccountNumberAsync(long portfolioId, string accountNumber, CancellationToken cancellationToken = default)
    {
        return await context.BankAccounts
            .AnyAsync(x => x.PortfolioId == portfolioId && x.AccountNumber == accountNumber, cancellationToken);
    }

}