using Microsoft.EntityFrameworkCore;
using Products.Domain.Banks;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Banks;

public class BankRepository(ProductsDbContext context) : IBankRepository
{
    public async Task<IReadOnlyCollection<Bank>> GetBanksAsync(CancellationToken cancellationToken = default)
    {
        return await context.Bank
            .Select(dt => Bank.CreateforGraphql(
                dt.Name,
                dt.BankId
            )).ToListAsync(cancellationToken);
    }
}
