using Closing.Domain.TrustYields;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.TrustSync;

public sealed class TrustSyncService : IDataSyncService
{
    //private readonly ITrustYieldRepository repository;


    //public TrustSyncService(ITrustYieldRepository repository)
    //{
    //    this.repository = repository;
    //}

    //public async Task<Result> ExecuteAsync(int portfolioId, DateOnly closingDate, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        var trusts = await repository.GetActiveTrustsByPortfolioAsync(portfolioId, cancellationToken);

    //        foreach (var trust in trusts)
    //        {
    //            var snapshot = new TrustSnapshotDto
    //            {
    //                FideicomisoId = trust.FideicomisoId,
    //                PortfolioId = portfolioId,
    //                FechaCierre = closingDate,
    //                SaldoPreCierre = trust.SaldoTotal,
    //                Capital = trust.Capital,
    //                RetencionContingente = trust.RetencionContingente,
    //                FechaProceso = _clock.Today()
    //            };

    //            await _repository.UpsertYieldTrustAsync(snapshot, cancellationToken);
    //        }

    //        return Result.Success();
    //    }
    //    catch (Exception ex)
    //    {
    //        return Result.Failure(new Error("DATASYNC_ERROR", ex.Message));
    //    }
    //}
}
