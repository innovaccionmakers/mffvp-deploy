using Microsoft.EntityFrameworkCore;
using Operations.Domain.OperationTypes;
using Operations.Infrastructure.Database;
using Common.SharedKernel.Domain;

namespace Operations.Infrastructure.OperationTypes;

internal sealed class OperationTypeRepository(OperationsDbContext context) : IOperationTypeRepository
{
    public Task<OperationType?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken ct = default)
    {
        return context.OperationTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.HomologatedCode == homologatedCode, ct);
    }

    public Task<OperationType?> GetByNameAsync(
        string name,
        CancellationToken ct = default)
    {
        return context.OperationTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Name == name, ct);
    }

    public Task<OperationType?> GetByNameAndCategoryAsync(
        string name,
        int? categoryId,
        CancellationToken ct = default)
    {
        return context.OperationTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Name == name && s.CategoryId == categoryId, ct);
    }

    public async Task<IReadOnlyCollection<OperationType>> GetCategoryIdAsync(int? categoryId, CancellationToken ct = default)
    {
        return await context.OperationTypes
            .AsNoTracking()
            .Where(s => s.CategoryId == categoryId && s.Status == Status.Active && s.Visible)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<OperationType>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.OperationTypes
            .AsNoTracking()
            .Where(s => s.Status == Status.Active && s.Visible)
            .ToListAsync(ct);
    }
}