using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Closing.Application.Closing.Services.OperationTypes;
using Closing.Domain.TrustYields;
using Common.SharedKernel.Domain.OperationTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Closing.Application.PostClosing.Services.TrustYield;

/// <summary>
/// Processor que coordina las llamadas remotas:
/// 1) Operations.UpsertYieldOperation
/// 2) Trusts.UpdateFromYield (solo si 1) fue OK)
/// </summary>
public sealed class TrustYieldProcessor : ITrustYieldProcessor
{
    private readonly ITrustYieldRepository trustYieldRepository;
    private readonly IUpsertTrustYieldOperationsRemote operationsRemote;
    private readonly IUpdateTrustRemote trustsRemote;
    private readonly ILogger<TrustYieldProcessor> logger;
    private readonly TrustYieldRpcOptions options;
    private readonly IOperationTypesService operationTypes;

    public TrustYieldProcessor(
        ITrustYieldRepository trustYieldRepository,
        IUpsertTrustYieldOperationsRemote operationsRemote,
        IUpdateTrustRemote trustsRemote,
        ILogger<TrustYieldProcessor> logger,
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

    public async Task ProcessAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        logger.LogInformation("{Class} - Inicio (best-effort). Portfolio:{PortfolioId}, ClosingDate:{ClosingDate}, MDOP:{MDOP}, UseEmitFilter:{Filter}",
            nameof(TrustYieldProcessor), portfolioId, closingDate, options.MaxDegreeOfParallelism, options.UseEmitFilter);

        var subtypeResult = await operationTypes.GetAllAsync(cancellationToken);
        if (!subtypeResult.IsSuccess)
            return;

        var yieldOperationTypeId = subtypeResult.Value
            .Where(s => s.Nature == IncomeEgressNature.Income && string.IsNullOrWhiteSpace(s.Category) && s.Name == "Rendimientos")
            .Select(s => s.OperationTypeId)
            .SingleOrDefault();

        var trustYields = await trustYieldRepository
            .GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);

        if (trustYields.Count == 0)
        {
            logger.LogInformation("{Class} - No hay rendimientos para procesar.", nameof(TrustYieldProcessor));
            return;
        }

        var operationsOk = 0;
        var trustsOk = 0;
        var skipped = 0;
        var warnings = new ConcurrentBag<string>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(trustYields, parallelOptions, async (trustYield, ct) =>
        {
            if (options.UseEmitFilter)
            {
                var shouldEmit =
                    trustYield.YieldAmount != 0m ||
                    trustYield.PreClosingBalance != trustYield.ClosingBalance ||
                    trustYield.PreClosingBalance == trustYield.Capital;

                if (!shouldEmit)
                {
                    Interlocked.Increment(ref skipped);
                    logger.LogDebug("{Class} - Skip TrustId:{TrustId} (sin cambios relevantes).",
                        nameof(TrustYieldProcessor), trustYield.TrustId);
                    return;
                }
            }

            // 1) Operations: upsert de operación de rendimiento
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
                logger.LogWarning("{Class} - {Msg}", nameof(TrustYieldProcessor), msg);
                return; // no intentamos Trusts si falla Operations
            }

            Interlocked.Increment(ref operationsOk);

            // 2) Trusts: aplicar rendimiento al saldo
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
                logger.LogWarning("{Class} - {Msg}", nameof(TrustYieldProcessor), msg);
                return;
            }

            Interlocked.Increment(ref trustsOk);
        });

        if (!warnings.IsEmpty)
        {
            logger.LogWarning("{Class} - Best-effort con advertencias. Total:{Total}, OpsOK:{OpsOk}, TrustOK:{TrustOk}, Skipped:{Skipped}, Warnings:{WarnCount}. Detalles: {Details}",
                nameof(TrustYieldProcessor),
                trustYields.Count, operationsOk, trustsOk, skipped, warnings.Count, string.Join(" | ", warnings));
        }
        else
        {
            logger.LogInformation("{Class} - Fin OK. Total:{Total}, OpsOK:{OpsOk}, TrustOK:{TrustOk}, Skipped:{Skipped}",
                nameof(TrustYieldProcessor),
                trustYields.Count, operationsOk, trustsOk, skipped);
        }
    }
}
