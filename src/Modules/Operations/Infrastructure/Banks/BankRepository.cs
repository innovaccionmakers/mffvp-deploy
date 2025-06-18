using Microsoft.EntityFrameworkCore;
using Operations.Domain.Banks;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.Banks;

internal sealed class BankRepository(OperationsDbContext context) : IBankRepository
{
    public Task<Bank?> FindByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default)
    {
        return context.Banks
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.HomologatedCode == homologatedCode, cancellationToken);
    }
}