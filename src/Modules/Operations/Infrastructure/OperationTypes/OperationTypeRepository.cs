using Common.SharedKernel.Core.Primitives;

using Microsoft.EntityFrameworkCore;

using Operations.Domain.OperationTypes;
using Operations.Infrastructure.Database;
using System.Text.Json;

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
            .Where(s => s.Name.Trim() == name.Trim())
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

    public async Task<IReadOnlyCollection<OperationType>> GetTypesByCategoryAsync(int? categoryId, CancellationToken ct = default,
         IEnumerable<string>? groupLists = null,
         bool? visible = true)
    {
        var query = context.OperationTypes
        .AsNoTracking()
        .Where(t => t.CategoryId == categoryId &&
                    t.Status == Status.Active);

        // Visible: solo se aplica si viene valor
        if (visible.HasValue)
            query = query.Where(t => t.Visible == visible.Value);

        // GrupoLista: 0, 1 o muchos
        if (groupLists is not null)
        {
            var normalized = groupLists
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                // Construye {"GrupoLista":"..."} para usar @> (containment)
                .Select(g => JsonSerializer.Serialize(new { GrupoLista = g }))
                .ToArray();

            if (normalized.Length > 0)
            {
                query = query.Where(t =>
                    normalized.Any(j => EF.Functions.JsonContains(t.AdditionalAttributes, j)));
            }
        }

        return await query
            .TagWith("OperationTypeRepository_GetTypesByCategoryAsync")
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<OperationType>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.OperationTypes
            .AsNoTracking()
            .Where(s => s.Status == Status.Active )
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