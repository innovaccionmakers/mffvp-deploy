

using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Common.SharedKernel.Domain;
using Consul;
using DotNetCore.CAP;

namespace Closing.Application.Closing.Services.TrustYieldsDistribution;

public class DistributeTrustYieldsService : IDistributeTrustYieldsService
{
    private readonly ITrustYieldRepository _trustYieldRepository;
    private readonly IYieldRepository _yieldRepository;
    private readonly IPortfolioValuationRepository _portfolioValuationRepository;
    private readonly ICapPublisher _capPublisher;
    //private readonly IBusinessParameterProvider _businessParameterProvider;

    public DistributeTrustYieldsService(
        ITrustYieldRepository trustYieldRepository,
        IYieldRepository yieldRepository,
        IPortfolioValuationRepository portfolioValuationRepository,
        ICapPublisher capPublisher//,
       // IBusinessParameterProvider businessParameterProvider
        )

    {
        _trustYieldRepository = trustYieldRepository;
        _yieldRepository = yieldRepository;
        _portfolioValuationRepository = portfolioValuationRepository;
        _capPublisher = capPublisher;
       // _businessParameterProvider = businessParameterProvider;
    }

    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await _capPublisher.PublishAsync("closingExecution", new
        {
            processDatetime = now,
            process = "ClosingAllocation"
        });

        var yield = await _yieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (yield is null)
            return Result.Failure(new Error("001","No se encontró información de rendimientos para la fecha de cierre.",ErrorType.Failure));

        var portfolioValuation = await _portfolioValuationRepository
            .GetValuationAsync(portfolioId, closingDate, ct);
        if (portfolioValuation is null)
            return Result.Failure(new Error("002","No existe valoración del portafolio para la fecha indicada.",ErrorType.Failure));

        var trusts = await _trustYieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (!trusts.Any())
            return Result.Failure(new Error("003","No existen registros en rendimientos_fideicomisos para esta fecha. Debe reprocesarse la réplica de datos.",ErrorType.Failure));

        var retentionRate = 1;//await _businessParameterProvider.GetRetentionRateAsync(ct);

        foreach (var trust in trusts)
        {
            if (trust.Participation == 0m)
                continue;

            var rendimiento = yield.YieldToCredit * trust.Participation;
            var income = yield.Income * trust.Participation;
            var gastos = yield.Expenses * trust.Participation;
            var comisiones = yield.Commissions * trust.Participation;
            var costo = yield.Costs * trust.Participation;
            var saldoCierre = trust.PreClosingBalance + rendimiento;

            decimal unidades = 0m;
            if (trust.PreClosingBalance != trust.ClosingBalance)
                unidades = Math.Round(saldoCierre / portfolioValuation.UnitValue, 16);

            var retencionRendimiento = Math.Round(rendimiento * retentionRate, 16);

            trust.UpdateDetails(
                trust.TrustId,
                trust.PortfolioId,
                trust.ClosingDate,
                trust.Participation,
                unidades,
                rendimiento,
                trust.PreClosingBalance,
                saldoCierre,
                income,
                gastos,
                comisiones,
                costo,
                trust.Capital,
                now,
                trust.ContingentRetention,
                retencionRendimiento
            );
        }

        await _trustYieldRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}