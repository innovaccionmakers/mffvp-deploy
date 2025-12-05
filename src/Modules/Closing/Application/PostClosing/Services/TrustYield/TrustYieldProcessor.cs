using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Closing.Application.PostClosing.Services.PortfolioServices;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.TrustYields;
using Common.SharedKernel.Application.Constants;
using Common.SharedKernel.Application.Constants.Closing;
using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Helpers.Money;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Domain.OperationTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Closing.Application.PostClosing.Services.TrustYield;

/// <summary>
/// Processor que coordina llamadas remotas en BULK con chunking unificado:
/// 1) Operations.UpsertYieldOperationsBulk (por lotes)
/// 2) Trusts.UpdateFromYield (por lotes) SOLO para TrustIds cambiados en 1
/// </summary>
public sealed class TrustYieldProcessor : ITrustYieldProcessor
{
    private readonly ITrustYieldRepository trustYieldRepository;
    private readonly IUpsertTrustYieldOperationsRemote operationsRemote;
    private readonly IUpdateTrustRemote trustsRemote;
    private readonly ILogger<TrustYieldProcessor> logger;
    private readonly TrustYieldOptions options;
    private readonly IOperationTypesLocator operationTypesLocator;
    private readonly IConfigurationParameterRepository configurationParameterRepository;
    private readonly IPortfolioService portfolioService;

    private const int DefaultChunkSize = ClosingBulkProperties.BulkBatchSize;
    private const int DefaultMaxConcurrency = 8;

    public TrustYieldProcessor(
        ITrustYieldRepository trustYieldRepository,
        IUpsertTrustYieldOperationsRemote operationsRemote,
        IUpdateTrustRemote trustsRemote,
        ILogger<TrustYieldProcessor> logger,
        IOperationTypesLocator operationTypesLocator,
        IOptions<TrustYieldOptions> options,
        IConfigurationParameterRepository configurationParameterRepository,
        IPortfolioService portfolioService)
    {
        this.trustYieldRepository = trustYieldRepository;
        this.operationsRemote = operationsRemote;
        this.trustsRemote = trustsRemote;
        this.logger = logger;
        this.options = options.Value ?? new TrustYieldOptions();
        this.operationTypesLocator = operationTypesLocator;
        this.configurationParameterRepository = configurationParameterRepository;
        this.portfolioService = portfolioService;
    }

    public async Task ProcessAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken)
    {
        // 0) Tipo operación "Rendimientos"
        var subtypeResult = await operationTypesLocator.GetOperationTypeByNameAsync(OperationTypeAttributes.Names.Yields, cancellationToken);
        if (!subtypeResult.IsSuccess || subtypeResult.Value.OperationTypeId <= 0)
        {
            logger.LogWarning("{Class} - Tipo de Operación 'Rendimientos' no disponible: {Error}",
                nameof(TrustYieldProcessor), subtypeResult.IsSuccess ? "OperationTypeId<=0" : subtypeResult.Error?.Description);
            return;
        }
        var yieldOperationTypeId = subtypeResult.Value.OperationTypeId;

        // 1) Leer TrustYields del día
        var trustYields = await trustYieldRepository
            .GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDateUtc, cancellationToken);

        if (trustYields.Count == 0)
        {
            logger.LogInformation("{Class} - No hay rendimientos de Fideicomisos para procesar.", nameof(TrustYieldProcessor));
            return;
        }

        // 2) Filtro de emisión
        IEnumerable<Domain.TrustYields.TrustYield> candidatesEnum = trustYields;
        if (options.UseEmitFilter)
        {
            candidatesEnum = trustYields.Where(t =>
                   t.YieldAmount != 0m
                || t.PreClosingBalance != t.ClosingBalance
                || t.PreClosingBalance == t.Capital);
        }

        var candidates = candidatesEnum as List<Domain.TrustYields.TrustYield> ?? candidatesEnum.ToList();
        if (candidates.Count == 0)
        {
            logger.LogInformation("{Class} - Todos los rendimientos fueron omitidos por filtro de emisión.",
                nameof(TrustYieldProcessor));
            return;
        }

        var idempotencyKeyBase = $"{portfolioId}:{closingDateUtc:yyyyMMdd}";
        var chunkSize = options.BulkBatchSize > 0 ? options.BulkBatchSize : DefaultChunkSize;
        var maxConcurrency = DefaultMaxConcurrency;

        // 3) Operations BULK 
        var opItems = candidates.Select(t => new TrustYieldOperation(
            TrustId: t.TrustId,
            OperationTypeId: yieldOperationTypeId,
            Amount: t.YieldAmount,
            ClientOperationId: null,
            ProcessDateUtc: closingDateUtc
        )).ToArray();

        var opChunks = ChunkWithIndex(opItems, chunkSize);
        var changedTrusts = new ConcurrentDictionary<long, byte>();
        int totalOpsInserted = 0, totalOpsUpdated = 0;

        await ForEachAsyncBounded(opChunks, maxConcurrency, async (batch, batchIndex, cancellationToken) =>
        {
            var req = new UpsertTrustYieldOperationsBulkRemoteRequest(
                PortfolioId: portfolioId,
                ClosingDateUtc: closingDateUtc,
                OperationTypeId: yieldOperationTypeId,
                TrustYieldOperations: batch,
                IdempotencyKey: $"{idempotencyKeyBase}-ops-b{batchIndex}"
            );

            var res = await operationsRemote.UpsertYieldOperationsBulkAsync(req, cancellationToken);
            if (res.IsFailure)
            {
                logger.LogWarning("{Class} - OPS_BULK_FAIL Lote:{Batch} {Code} {Msg}",
                    nameof(TrustYieldProcessor), batchIndex, res.Error.Code, res.Error.Description);
                return;
            }

            var val = res.Value;
            Interlocked.Add(ref totalOpsInserted, val.Inserted);
            Interlocked.Add(ref totalOpsUpdated, val.Updated);

            if (val.ChangedTrustIds is { Count: > 0 })
            {
                foreach (var id in val.ChangedTrustIds)
                    changedTrusts.TryAdd(id, 0);
            }

            logger.LogInformation("{Class} - Operations OK Lote:{Batch}. Inserted:{Inserted} Updated:{Updated} ChangedTrusts:{Changed}",
                nameof(TrustYieldProcessor), batchIndex, val.Inserted, val.Updated, val.ChangedTrustIds?.Count ?? 0);
        }, cancellationToken);

        logger.LogInformation("{Class} - Operations COMPLETADO. Total Inserted:{Inserted} Total Updated:{Updated} Total ChangedTrusts:{Changed}",
            nameof(TrustYieldProcessor), totalOpsInserted, totalOpsUpdated, changedTrusts.Count);

        // 4) Trusts BULK (sólo los que cambiaron)
        if (changedTrusts.IsEmpty)
        {
            logger.LogInformation("{Class} - No hubo cambios en Operations; no se invoca Trusts.", nameof(TrustYieldProcessor));
            return;
        }

        // Parámetro de retención
        var param = await configurationParameterRepository.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldRetentionPercentage, cancellationToken);
        if (param is null)
        { 
            logger.LogWarning("{Class} - No se encuentra configurado el parámetro de retención de rendimiento",
                  nameof(TrustYieldProcessor));
            return;
        }
        var yieldRetentionRate = JsonDecimalHelper.ExtractDecimal(param.Metadata, "valor", true);
        if (yieldRetentionRate <= 0)
        {
            logger.LogWarning("{Class} - El parametro de retención de rendimientos no es un valor mayor a 0",
                  nameof(TrustYieldProcessor));
            return;
        }

        var AgileWithdrawalPercentageProtectedBalance = await portfolioService.GetAsync(
            portfolioId,
            cancellationToken);

        var AgileWithdrawalRateProtectedBalance = AgileWithdrawalPercentageProtectedBalance / 100m;

        var trustItemsAll = candidates
            .Where(t => changedTrusts.ContainsKey(t.TrustId))
            .Select(t => new UpdateTrustFromYieldItem(
                TrustId: t.TrustId,
                YieldAmount: t.YieldAmount,
                YieldRetentionRate: yieldRetentionRate,
                ClosingBalance: t.ClosingBalance
            ))
            .ToArray();

        if (trustItemsAll.Length == 0)
        {
            logger.LogInformation("{Class} - No hay fideicomisos para actualizar luego del filtro de cambios en Operations.",
                nameof(TrustYieldProcessor));
            return;
        }

        var trChunks = ChunkWithIndex(trustItemsAll, chunkSize);
        int totalUpdated = 0, totalMissing = 0, totalMismatch = 0;

        await ForEachAsyncBounded(trChunks, maxConcurrency, async (batch, batchIndex, cancellationToken) =>
        {
            var request = new UpdateTrustFromYieldBulkRemoteRequest(
                PortfolioId: portfolioId,
                ClosingDate: closingDateUtc,
                BatchIndex: batchIndex,
                TrustsToUpdate: batch, 
                IdempotencyKey: $"{idempotencyKeyBase}-tr-b{batchIndex}",
                AgileWithdrawalPercentageProtectedBalance: AgileWithdrawalRateProtectedBalance
            );

            var res = await trustsRemote.UpdateFromYieldAsync(request, cancellationToken);
            if (res.IsFailure)
            {
                logger.LogWarning("{Class} - TRUST_BULK_FAIL Lote:{Batch} {Code} {Msg}",
                    nameof(TrustYieldProcessor), batchIndex, res.Error.Code, res.Error.Description);
                return;
            }

            var v = res.Value;
            Interlocked.Add(ref totalUpdated, v.Updated);
            Interlocked.Add(ref totalMissing, v.MissingTrustIds?.Count ?? 0);
            Interlocked.Add(ref totalMismatch, v.ValidationMismatchTrustIds?.Count ?? 0);

            logger.LogInformation("{Class} - Trusts OK Lote:{Batch}. Actualizados:{Updated} Perdidos:{Missing} No coinciden:{Mismatch}",
                nameof(TrustYieldProcessor),
                v.BatchIndex, v.Updated, v.MissingTrustIds?.Count ?? 0, v.ValidationMismatchTrustIds?.Count ?? 0);
        }, cancellationToken);

        logger.LogInformation("{Class} - Trusts COMPLETADO. Total Actualizados:{Updated} Perdidos:{Missing} No coinciden:{Mismatch}",
            nameof(TrustYieldProcessor), totalUpdated, totalMissing, totalMismatch);
    }

    // Helpers

    private static (T[] Batch, int Index)[] ChunkWithIndex<T>(T[] source, int chunkSize)
    {
        if (source.Length == 0) return Array.Empty<(T[] Batch, int Index)>();
        var chunks = new List<(T[] Batch, int Index)>(Math.Max(1, (source.Length + chunkSize - 1) / chunkSize));
        var index = 0;
        for (int i = 0; i < source.Length; i += chunkSize, index++)
        {
            var count = Math.Min(chunkSize, source.Length - i);
            var slice = new T[count];
            Array.Copy(source, i, slice, 0, count);
            chunks.Add((slice, index));
        }
        return chunks.ToArray();
    }

    private static async Task ForEachAsyncBounded<T>(
        (T[] Batch, int Index)[] items,
        int maxConcurrency,
        Func<T[], int, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        using var gate = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = new List<Task>(items.Length);

        foreach (var (batch, index) in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await gate.WaitAsync(cancellationToken);
            tasks.Add(Task.Run(async () =>
            {
                try { await handler(batch, index, cancellationToken); }
                finally { gate.Release(); }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}