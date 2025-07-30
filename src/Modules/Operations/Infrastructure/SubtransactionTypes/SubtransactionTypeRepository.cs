using Microsoft.EntityFrameworkCore;
using Operations.Domain.SubtransactionTypes;
using Operations.Infrastructure.Database;
using Common.SharedKernel.Domain;

namespace Operations.Infrastructure.SubtransactionTypes;

internal sealed class SubtransactionTypeRepository(OperationsDbContext context) : ISubtransactionTypeRepository
{
    public Task<SubtransactionType?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken ct = default)
    {
        return context.SubtransactionTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.HomologatedCode == homologatedCode, ct);
    }

    public Task<SubtransactionType?> GetByNameAsync(
        string name,
        CancellationToken ct = default)
    {
        return context.SubtransactionTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Name == name, ct);
    }

    public Task<SubtransactionType?> GetByNameAndCategoryAsync(
        string name,
        Guid categoryId,
        CancellationToken ct = default)
    {
        return context.SubtransactionTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Name == name && s.Category == categoryId, ct);
    }

    public async Task<IReadOnlyCollection<SubtransactionType>> GetCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await context.SubtransactionTypes
            .AsNoTracking()
            .Where(s => s.Category == categoryId && s.Status == Status.Active)
            .ToListAsync(ct);
    }
    
    public async Task<IReadOnlyCollection<SubtransactionType>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.SubtransactionTypes
            .AsNoTracking()
            .Where(s => s.Status == Status.Active)
            .ToListAsync(ct);
    }
}