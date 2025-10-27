using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Closing.Application.Closing.Services.OperationTypes;
using Closing.Domain.TrustYields;
using Common.SharedKernel.Domain.OperationTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Linq;

namespace Closing.Application.PostClosing.Services.TrustYield;

public sealed class TrustYieldBulkProcessor : ITrustYieldBulkProcessor
{
    private readonly ITrustYieldRepository trustYieldRepository;
    private readonly IUpsertTrustYieldOperationsRemote operationsRemote;
    private readonly IUpdateTrustRemote trustsRemote;
    private readonly ILogger<TrustYieldBulkProcessor> logger;
    private readonly TrustYieldRpcOptions options;
    private readonly IOperationTypesService operationTypes;

    public TrustYieldBulkProcessor(
        ITrustYieldRepository trustYieldRepository,
        IUpsertTrustYieldOperationsRemote operationsRemote,
        IUpdateTrustRemote trustsRemote,
        ILogger<TrustYieldBulkProcessor> logger,
        IOperationTypesService operationTypes,
        IOptions<TrustYieldRpcOptions> options)
    {
        this.trustYieldRepository = trustYieldRepository;
        this.operationsRemote = operationsRemote;
        this.trustsRemote = trustsRemote;
        this.logger = logger;
        this.options = options.Value ?? new TrustYieldRpcOptions();
        this.operationTypes = operationTypes;
    }

    public async Task ProcessAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{Class} - Inicio. Portfolio:{PortfolioId}, ClosingDate:{ClosingDate}, UseBulk:{UseBulk}, MDOP:{MDOP}, UseEmitFilter:{Filter}",
            nameof(TrustYieldProcessor), portfolioId, closingDateUtc, options.UseBulkRpc, options.MaxDegreeOfParallelism, options.UseEmitFilter);

        var subtypeResult = await operationTypes.GetAllAsync(cancellationToken);
        if (!subtypeResult.IsSuccess)
        {
            logger.LogWarning("{Class} - No se pudo obtener OperationTypes: {Error}", nameof(TrustYieldProcessor), subtypeResult.Error?.Description);
            return;
        }

        var yieldOperationTypeId = subtypeResult.Value
            .Where(s => s.Nature == IncomeEgressNature.Income && string.IsNullOrWhiteSpace(s.Category) && s.Name == "Rendimientos")
            .Select(s => s.OperationTypeId)
            .SingleOrDefault();

        if (yieldOperationTypeId <= 0)
        {
            logger.LogWarning("{Class} - OperationType 'Rendimientos' no encontrado.", nameof(TrustYieldProcessor));
            return;
        }
        var allTrustYields = await trustYieldRepository
            .GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDateUtc, cancellationToken);

        if (allTrustYields.Count == 0)
        {
            logger.LogInformation("{Class} - No hay rendimientos para procesar.", nameof(TrustYieldProcessor));
            return;
        }

        // Filtro previo para no incluir ítems que no emiten
        var candidates = options.UseEmitFilter
            ? allTrustYields.Where(ShouldEmit).ToList()
            : allTrustYields;

        if (candidates.Count == 0)
        {
            logger.LogInformation("{Class} - Todos los rendimientos fueron omitidos por filtro de emisión.", nameof(TrustYieldProcessor));
            return;
        }

        // Idempotencia a nivel de corrida (ideal: usa ExecutionId del orquestador si lo tienes)
        var executionId = options.ExecutionId == Guid.Empty ? Guid.NewGuid() : options.ExecutionId;
        var idempotencyKey = $"{portfolioId}:{closingDateUtc:yyyyMMdd}";

        if (options.UseBulkRpc)
        {
            await ProcessInBulkAsync((IReadOnlyList<Domain.TrustYields.TrustYield>)candidates, portfolioId, closingDateUtc, yieldOperationTypeId, idempotencyKey, executionId, cancellationToken);
        }
        else
        {
            await ProcessIndividuallyAsync((IReadOnlyList<Domain.TrustYields.TrustYield>)candidates, yieldOperationTypeId, cancellationToken);
        }
    }

    private static bool ShouldEmit(Domain.TrustYields.TrustYield trustYield)
    {
        // Mantiene tu lógica actual
        return trustYield.YieldAmount != 0m
            || trustYield.PreClosingBalance != trustYield.ClosingBalance
            || trustYield.PreClosingBalance == trustYield.Capital;
    }

    private async Task ProcessInBulkAsync(
        IReadOnlyList<Domain.TrustYields.TrustYield> items,
        int portfolioId,
        DateTime closingDateUtc,
        long yieldOperationTypeId,
        string idempotencyKey,
        Guid executionId,
        CancellationToken cancellationToken)
    {
        var batchSize = Math.Max(1, options.BulkBatchSize);
        var total = items.Count;

        var operationsOk = 0;
        var trustsOk = 0;
        var skipped = 0;

        var warnings = new List<string>(capacity: Math.Min(total, options.BulkWarningsLimit));
        int processed = 0;

        foreach (var batch in items.Chunk(batchSize))
        {
            // 1) Operations (bulk)
            var opItems = batch.Select(t => new TrustYieldOperationItem(
                TrustId: t.TrustId,
                OperationTypeId: yieldOperationTypeId,
                YieldAmount: t.YieldAmount,
                YieldRetention: t.YieldRetention,
                ApplicationDateUtc: t.ClosingDate,
                ProcessDateUtc: t.ProcessDate,
                ClosingBalance: t.ClosingBalance
            )).ToList();

            var opReq = new UpsertTrustYieldOperationsBulkRequest(
                PortfolioId: portfolioId,
                ClosingDateUtc: closingDateUtc,
                Items: opItems,
                IdempotencyKey: idempotencyKey,
                ExecutionId: executionId);

            var opRes = await operationsRemote.UpsertYieldOperationsBulkAsync(opReq, cancellationToken);
            if (opRes.IsFailure)
            {
                // Si falla todo el lote, registra y pasa al siguiente (best-effort)
                AddWarning(warnings, options.BulkWarningsLimit,
                    $"OPS_BULK_FAIL Code:{opRes.Error.Code} Msg:{opRes.Error.Description}");
                continue;
            }

            operationsOk += opRes.Value.SucceededTrustIds.Count;
            skipped += opRes.Value.Failures.Count;

            // 2) Trusts sólo con los que pasaron Operations
            if (opRes.Value.SucceededTrustIds.Count > 0)
            {
                var okSet = new HashSet<long>(opRes.Value.SucceededTrustIds);
                var trustBatch = batch
                    .Where(t => okSet.Contains(t.TrustId))
                    .Select(t => (t.TrustId, t.YieldAmount, t.YieldRetention, t.ClosingBalance))
                    .ToList();

                var trReq = new UpdateTrustFromYieldBulkRequest(
                    PortfolioId: portfolioId,
                    ClosingDateUtc: closingDateUtc,
                    Items: trustBatch,
                    IdempotencyKey: idempotencyKey,
                    ExecutionId: executionId);

                var trRes = await trustsRemote.UpdateFromYieldBulkAsync(trReq, cancellationToken);
                if (trRes.IsFailure)
                {
                    AddWarning(warnings, options.BulkWarningsLimit,
                        $"TRUST_BULK_FAIL Code:{trRes.Error.Code} Msg:{trRes.Error.Description}");
                }
                else
                {
                    trustsOk += trRes.Value.Updated;

                    // Mezcla warnings por ítem si vienen
                    foreach (var fail in trRes.Value.Failures.Take(Math.Max(0, options.BulkWarningsLimit - warnings.Count)))
                    {
                        AddWarning(warnings, options.BulkWarningsLimit,
                            $"TRUST_FAIL TrustId:{fail.TrustId} Code:{fail.Code} Msg:{fail.Message}");
                    }
                }
            }

            // Mezcla warnings por ítem de Operations si vinieron
            foreach (var fail in opRes.Value.Failures.Take(Math.Max(0, options.BulkWarningsLimit - warnings.Count)))
            {
                AddWarning(warnings, options.BulkWarningsLimit,
                    $"OPS_FAIL TrustId:{fail.TrustId} Code:{fail.Code} Msg:{fail.Message}");
            }

            processed += batch.Length;
            logger.LogInformation("{Class} - Progreso bulk {Processed}/{Total} (OpsOK:{OpsOk}, TrustOK:{TrustOk}, Skipped:{Skipped})",
                nameof(TrustYieldProcessor), processed, total, operationsOk, trustsOk, skipped);
        }

        if (warnings.Count > 0)
        {
            logger.LogWarning("{Class} - Best-effort con advertencias. Total:{Total}, OpsOK:{OpsOk}, TrustOK:{TrustOk}, Skipped:{Skipped}, Warnings:{WarnCount}.",
                nameof(TrustYieldProcessor), total, operationsOk, trustsOk, skipped, warnings.Count);
            logger.LogDebug("{Class} - Warnings: {Warnings}", nameof(TrustYieldProcessor), string.Join(" | ", warnings));
        }
        else
        {
            logger.LogInformation("{Class} - Fin OK. Total:{Total}, OpsOK:{OpsOk}, TrustOK:{TrustOk}, Skipped:{Skipped}",
                nameof(TrustYieldProcessor), total, operationsOk, trustsOk, skipped);
        }
    }

    private async Task ProcessIndividuallyAsync(
        IReadOnlyList<Domain.TrustYields.TrustYield> items,
        long yieldOperationTypeId,
        CancellationToken cancellationToken)
    {
        var operationsOk = 0;
        var trustsOk = 0;
        var skipped = 0;
        var warnings = new ConcurrentBag<string>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(items, parallelOptions, async (trustYield, ct) =>
        {
            // 1) Operations
            var opReq = new UpsertTrustYieldOperationRemoteRequest(
                TrustId: trustYield.TrustId,
                PortfolioId: trustYield.PortfolioId,
                ClosingDate: trustYield.ClosingDate,
                OperationTypeId: yieldOperationTypeId,
                YieldAmount: trustYield.YieldAmount,
                YieldRetention: trustYield.YieldRetention,
                ProcessDate: trustYield.ProcessDate,
                ClosingBalance: trustYield.ClosingBalance
            );

            var opRes = await operationsRemote.UpsertYieldOperationAsync(opReq, ct);
            if (opRes.IsFailure)
            {
                var msg = $"OPS_FAIL TrustId:{trustYield.TrustId} Code:{opRes.Error.Code} Msg:{opRes.Error.Description}";
                warnings.Add(msg);
                return;
            }
            Interlocked.Increment(ref operationsOk);

            // 2) Trusts
            var trReq = new UpdateTrustFromYieldRemoteRequest(
                PortfolioId: trustYield.PortfolioId,
                ClosingDate: trustYield.ClosingDate,
                TrustId: trustYield.TrustId,
                YieldAmount: trustYield.YieldAmount,
                YieldRetention: trustYield.YieldRetention,
                ClosingBalance: trustYield.ClosingBalance
            );

            var trRes = await trustsRemote.UpdateFromYieldAsync(trReq, ct);
            if (trRes.IsFailure)
            {
                var msg = $"TRUST_FAIL TrustId:{trustYield.TrustId} Code:{trRes.Error.Code} Msg:{trRes.Error.Description}";
                warnings.Add(msg);
                return;
            }
            Interlocked.Increment(ref trustsOk);
        });

        if (!warnings.IsEmpty)
        {
            // Limita spam
            var list = warnings.Take(options.BulkWarningsLimit).ToList();
            logger.LogWarning("{Class} - Best-effort con advertencias. Total:{Total}, OpsOK:{OpsOk}, TrustOK:{TrustOk}, Skipped:{Skipped}, Warnings mostrados:{Shown}/{All}",
                nameof(TrustYieldProcessor), items.Count, operationsOk, trustsOk, skipped, list.Count, warnings.Count);
            logger.LogDebug("{Class} - Warnings: {Warnings}", nameof(TrustYieldProcessor), string.Join(" | ", list));
        }
        else
        {
            logger.LogInformation("{Class} - Fin OK. Total:{Total}, OpsOK:{OpsOk}, TrustOK:{TrustOk}, Skipped:{Skipped}",
                nameof(TrustYieldProcessor), items.Count, operationsOk, trustsOk, skipped);
        }
    }

    private static void AddWarning(List<string> warnings, int limit, string msg)
    {
        if (warnings.Count < limit) warnings.Add(msg);
    }
}
