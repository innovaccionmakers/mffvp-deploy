using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Domain.YieldDetails;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Closing.Infrastructure.YieldDetails
{
    internal sealed class YieldDetailRepository(ClosingDbContext context, IDbContextFactory<ClosingDbContext> dbFactory) : IYieldDetailRepository
    {

        public async Task InsertAsync(YieldDetail yieldDetail, CancellationToken ct = default)
        {
            await context.YieldDetails.AddAsync(yieldDetail, ct);
        }

        public async Task DeleteByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            var deletedCount = await db.YieldDetails
               .TagWith($"YieldDetailRepository_DeleteByPortfolioAndDateAsync({portfolioId}, {closingDateUtc:yyyy-MM-dd})")
               .Where(yield => yield.PortfolioId == portfolioId
                             && yield.ClosingDate == closingDateUtc
                             && !yield.IsClosed //Solo se pueden borrar los que no son Cerrados
                            &&  yield.Source != YieldsSources.AutomaticConcept) // Excluir los conceptos automáticos calculados el dia anterior
                .ExecuteDeleteAsync(cancellationToken);

        }

        public async Task DeleteClosedByPortfolioAndDateAsync(
            int portfolioId,
            DateTime closingDateUtc,
            CancellationToken cancellationToken = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            await db.YieldDetails
                .TagWith($"YieldDetailRepository_DeleteClosedByPortfolioAndDateAsync({portfolioId}, {closingDateUtc:yyyy-MM-dd})")
                .Where(yield => yield.PortfolioId == portfolioId
                                && yield.ClosingDate == closingDateUtc
                                && yield.IsClosed
                                && yield.Source != YieldsSources.AutomaticConcept) // Excluir los conceptos automáticos calculados el dia anterior

                .ExecuteDeleteAsync(cancellationToken);

        }

        public async Task<IReadOnlyCollection<YieldDetail>> GetReadOnlyByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        bool isClosed = false,
        CancellationToken cancellationToken = default)
        {
            var baseQuery = context.YieldDetails
            .TagWith($"YieldDetailRepository_GetReadOnlyByPortfolioAndDateAsync({portfolioId}, {closingDateUtc:yyyy-MM-dd})")
            .AsNoTracking()
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate == closingDateUtc);

            if (isClosed)
            {
                // Solo cerrados
                return await baseQuery
                    .Where(y => y.IsClosed)
                    .ToListAsync(cancellationToken);
            }

            // Abiertos + cerrados por Conceptos Automáticos
            var openQ = baseQuery.Where(y => !y.IsClosed);
            var closedAutoQ = baseQuery.Where(y => y.IsClosed && y.Source == YieldsSources.AutomaticConcept);

            return await openQ
                .Union(closedAutoQ)
                .OrderBy(y => y.YieldDetailId)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByPortfolioAndDateAsync(
            int portfolioId,
            DateTime closingDateUtc,
            bool isClosed = false,
            CancellationToken ct = default)
        {
            return await context.YieldDetails.AsNoTracking()
                .AnyAsync(y => y.PortfolioId == portfolioId &&
                               y.ClosingDate == closingDateUtc &&
                               y.IsClosed == isClosed, ct);
        }

        public async Task<int> InsertRangeImmediateAsync(IReadOnlyList<YieldDetail> items, CancellationToken cancellationToken = default)
        {
            if (items is null || items.Count == 0) return 0;

            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

            await db.YieldDetails.AddRangeAsync(items, cancellationToken);
            var inserted = await db.SaveChangesAsync(cancellationToken);

            return inserted;
        }

        public async Task<IReadOnlyCollection<YieldDetail>> GetYieldDetailsAutConceptsAsync(IEnumerable<int> portfolioIdIds, DateTime closeDate, CancellationToken cancellationToken = default)
        {
            return await context.YieldDetails
                .AsNoTracking()
                .Where(y => portfolioIdIds.Contains(y.PortfolioId) && y.ClosingDate == closeDate && y.Source == YieldsSources.AutomaticConcept)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<YieldDetail>> GetYieldDetailsByPortfolioIdsAndClosingDateAsync(IEnumerable<int> portfolioIds, DateTime closingDate, string source, string? conceptJson, CancellationToken cancellationToken = default)
        {
            var query = context.YieldDetails
                .AsNoTracking()
                .Where(y => portfolioIds.Contains(y.PortfolioId)
                    && y.ClosingDate == closingDate
                    && y.IsClosed
                    && y.Source == source);

            if (!string.IsNullOrEmpty(conceptJson))
            {
                query = query.Where(y => EF.Functions.JsonContained(y.Concept, conceptJson));
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<YieldDetail>> GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptsAsync(IEnumerable<int> portfolioIds, DateTime closingDate, string source, IEnumerable<string> conceptJsons, CancellationToken cancellationToken = default)
        {
            var conceptJsonsList = conceptJsons?.Where(c => !string.IsNullOrEmpty(c)).ToList() ?? new List<string>();

            var query = context.YieldDetails
                .AsNoTracking()
                .Where(y => portfolioIds.Contains(y.PortfolioId)
                    && y.ClosingDate == closingDate
                    && y.IsClosed
                    && y.Source == source);
            
            
            var isInMemory = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

            if (conceptJsonsList.Count > 0)
            {
                if (isInMemory)
                {
                    var allResults = await query.ToListAsync(cancellationToken);
                 
                    return allResults
                        .Where(y => conceptJsonsList.Any(conceptJson => IsJsonContained(y.Concept, conceptJson)))
                        .ToList();
                }
                else
                {                    
                    query = query.Where(y => conceptJsonsList.Any(conceptJson => EF.Functions.JsonContained(y.Concept, conceptJson)));
                }
            }

            return await query.ToListAsync(cancellationToken);
        }

        private static bool IsJsonContained(JsonDocument? concept, string conceptJson)
        {
            if (concept == null) return false;

            try
            {
                var searchJson = JsonDocument.Parse(conceptJson);
                var conceptRoot = concept.RootElement;
                var searchRoot = searchJson.RootElement;

                if (conceptRoot.ValueKind == JsonValueKind.Object && searchRoot.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in searchRoot.EnumerateObject())
                    {
                        if (!conceptRoot.TryGetProperty(property.Name, out var conceptProperty))
                            return false;

                        if (!JsonElementEquals(conceptProperty, property.Value))
                            return false;
                    }
                    return true;
                }

                return JsonElementEquals(conceptRoot, searchRoot);
            }
            catch
            {
                return false;
            }
        }

        private static bool JsonElementEquals(JsonElement left, JsonElement right)
        {
            if (left.ValueKind != right.ValueKind)
                return false;

            return left.ValueKind switch
            {
                JsonValueKind.String => left.GetString() == right.GetString(),
                JsonValueKind.Number => left.GetRawText() == right.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => true,
                JsonValueKind.Null => true,
                JsonValueKind.Object => left.EnumerateObject().All(lp =>
                    right.TryGetProperty(lp.Name, out var rp) && JsonElementEquals(lp.Value, rp)),
                JsonValueKind.Array => left.EnumerateArray().SequenceEqual(right.EnumerateArray(), JsonElementComparer.Instance),
                _ => left.GetRawText() == right.GetRawText()
            };
        }

        private sealed class JsonElementComparer : IEqualityComparer<JsonElement>
        {
            public static readonly JsonElementComparer Instance = new();

            public bool Equals(JsonElement x, JsonElement y) => JsonElementEquals(x, y);

            public int GetHashCode(JsonElement obj) => obj.GetRawText().GetHashCode();
        }

        public async Task<decimal> GetExtraReturnIncomeSumAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default)
        {
            var query = context.YieldDetails
             .TagWith("YieldDetailRepository_GetExtraReturnIncomeSumAsync_PortfolioAndDate")
             .AsNoTracking()
             .Where(y => y.PortfolioId == portfolioId
                         && y.ClosingDate == closingDateUtc
                         && y.IsClosed
                         && y.Source == YieldsSources.ExtraReturn);

            var sum = await query
                .SumAsync(y => (decimal?)y.Income, cancellationToken);

            return sum ?? 0m;
        }
    }
}
