using Microsoft.EntityFrameworkCore;
using Operations.Domain.SubtransactionTypes;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.SubtransactionTypes;

internal sealed class SubtransactionTypeRepository(OperationsDbContext context) : ISubtransactionTypeRepository
{
    public Task<SubtransactionType?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken ct = default) =>
        context.SubtransactionTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.HomologatedCode == homologatedCode, ct);
    
    public Task<SubtransactionType?> GetByNameAsync(
        string name,
        CancellationToken ct = default) =>
        context.SubtransactionTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Name == name, ct);
}