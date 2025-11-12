using Common.SharedKernel.Core.Primitives;

using Microsoft.EntityFrameworkCore;

using Operations.Domain.OperationTypes;
using Operations.Infrastructure.Database;

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

    public async Task<IReadOnlyCollection<OperationType>> GetByNameAsync(
        string name,
        CancellationToken ct = default)
    {
        return await context.OperationTypes
            .AsNoTracking()
            .Where(s => s.Name == name)
            .ToListAsync(ct);
    }

    public async Task<OperationType?> GetByIdAsync(
        long operationTypeId,
        CancellationToken ct = default)
    {
        return await context.OperationTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.OperationTypeId == operationTypeId, ct);
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

    public async Task<IReadOnlyCollection<OperationType>> GetTypesByCategoryAsync(int? categoryId, CancellationToken ct = default)
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

    public async Task<IReadOnlyCollection<OperationType>> GetAccTransactionTypesAsync(CancellationToken cancellationToken = default)
    {
        return await context.OperationTypes
            .AsNoTracking()
            .Where(s => s.CategoryId == null && s.Visible)
            .ToListAsync(cancellationToken);
    }
}