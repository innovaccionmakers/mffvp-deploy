using Closing.Application.Closing.Services.Orchestation.Constants;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.PreClosing.Services.AutomaticConcepts.Dto;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;


namespace Closing.Application.Closing.Services.TrustYieldsDistribution;

public class ValidateTrustYieldsDistributionService(
    IYieldRepository yieldRepository,
    ITrustYieldRepository trustYieldRepository,
    IYieldDetailCreationService yieldDetailCreationService,
    YieldDetailBuilderService yieldDetailBuilderService,
    ITimeControlService timeControlService,
    ILogger<ValidateTrustYieldsDistributionService> logger)
    : IValidateTrustYieldsDistributionService
{
    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        using var _ = logger.BeginScope(new System.Collections.Generic.Dictionary<string, object>
        {
            ["Service"] = "ValidateTrustYieldsDistributionService",
            ["PortfolioId"] = portfolioId,
            ["ClosingDate"] = closingDate.Date
        });
        const string svc = "[ValidateTrustYieldsDistributionService]";

        logger.LogInformation("Validando distribución de rendimientos para portafolio {PortfolioId}", portfolioId);
        logger.LogInformation("{Svc} Inicio de validación de distribución de rendimientos.", svc);

        var now = DateTime.UtcNow;

        // Publicar evento de paso
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", now, ct);
        logger.LogInformation("{Svc} Paso de control de tiempo actualizado: NowUtc={NowUtc}", svc, now);

        var yield = await yieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (yield is null)
        {
            logger.LogWarning("{Svc} No se encontró información de rendimientos para {ClosingDate}", svc, closingDate);
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Yield obtenido: Income={Income}, Expenses={Expenses}, Commissions={Commissions}, Costs={Costs}, YieldToCredit={YieldToCredit}",
            svc, yield.Income, yield.Expenses, yield.Commissions, yield.Costs, yield.YieldToCredit);

        var trustYields = await trustYieldRepository.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (!trustYields.Any())
        {
            logger.LogWarning("{Svc} No existen rendimientos distribuidos para fideicomisos en {ClosingDate}", svc, closingDate);
            return Result.Failure(new Error("002", "No existen registros de rendimientos distribuidos para fideicomisos.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Registros de trust_yields encontrados: {Count}", svc, trustYields.Count);

        var distributedTotal = Math.Round(trustYields.Sum(x => x.YieldAmount), DecimalPrecision.TwoDecimals);
        var expectedTotal = yield.YieldToCredit;
        var difference = distributedTotal - expectedTotal;

        logger.LogInformation("Distribuido: {Distribuido}, Esperado: {Esperado}, Diferencia: {Diferencia}",
            distributedTotal, expectedTotal, difference);
        logger.LogInformation("{Svc} Totales calculados => DistributedTotal={DistributedTotal}, ExpectedTotal={ExpectedTotal}, Difference={Difference}",
            svc, distributedTotal, expectedTotal, difference);

        logger.LogInformation("{Svc} Actualizando Yield con creditedYields={CreditedYields}", svc, distributedTotal);
        yield.UpdateDetails(
            portfolioId: yield.PortfolioId,
            income: yield.Income,
            expenses: yield.Expenses,
            commissions: yield.Commissions,
            costs: yield.Costs,
            yieldToCredit: yield.YieldToCredit,
            creditedYields: distributedTotal,
            closingDate: yield.ClosingDate,
            processDate: yield.ProcessDate,
            isClosed: yield.IsClosed
        );

        await yieldRepository.SaveChangesAsync(ct);
        logger.LogInformation("{Svc} Cambios persistidos en yields.", svc);

        if (difference != 0m)
        {
            logger.LogInformation("{Svc} Se detectó diferencia distinta de 0. Se generará ajuste.", svc);

            var nextClosingDate = closingDate.AddDays(1);
            logger.LogInformation("{Svc} Próxima fecha de cierre para ajuste: {NextClosingDate}", svc, nextClosingDate);
            var isIncome = difference > 0m;
            var summary = new AutomaticConceptSummary(
                ConceptId: isIncome ? 1 : 2,
                ConceptName: isIncome ? "Ajuste Rendimiento Ingreso" : "Ajuste Rendimiento Gasto",
                Nature: isIncome ? IncomeExpenseNature.Income : IncomeExpenseNature.Expense,
                Source: "Concepto Automático",
                TotalAmount: difference
            );

            logger.LogInformation("{Svc} Concepto seleccionado para ajuste: Id={ConceptId}, Texto={ConceptText}, Nature={Nature}, Monto={TotalAmount}",
                svc, summary.ConceptId, summary.ConceptName, summary.Nature, summary.TotalAmount);

            var parameters = new RunSimulationParameters(
                PortfolioId: portfolioId,
                ClosingDate: nextClosingDate,
                IsClosing: true
            );

            var buildResult = yieldDetailBuilderService.Build(
                new[] { summary }, parameters);

            logger.LogInformation("{Svc} YieldDetail construido por builder. Procediendo a persistir.", svc);

            await yieldDetailCreationService.CreateYieldDetailsAsync(buildResult, ct);

            logger.LogInformation("Se generó ajuste de rendimiento para el portafolio {PortfolioId} en el día siguiente", portfolioId);
            logger.LogInformation("{Svc} Ajuste generado y persistido: Difference={Difference}, NextClosingDate={NextClosingDate}", svc, difference, nextClosingDate);
        }
        else
        {
            logger.LogInformation("No se detectaron diferencias en rendimientos distribuidos para portafolio {PortfolioId}", portfolioId);
            logger.LogInformation("{Svc} Sin diferencias: no se genera ajuste.", svc);
        }

        logger.LogInformation("{Svc} Validación de distribución finalizada correctamente.", svc);
        return Result.Success();
    }
}
