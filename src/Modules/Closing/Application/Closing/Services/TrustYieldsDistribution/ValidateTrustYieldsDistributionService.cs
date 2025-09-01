
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.AutomaticConcepts.Dto;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Constants;
using Common.SharedKernel.Application.Helpers.Money;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;



namespace Closing.Application.Closing.Services.TrustYieldsDistribution;

public class ValidateTrustYieldsDistributionService(
    IYieldRepository yieldRepository,
    ITrustYieldRepository trustYieldRepository,
    IYieldDetailCreationService yieldDetailCreationService,
    YieldDetailBuilderService yieldDetailBuilderService,
    ITimeControlService timeControlService,
    IConfigurationParameterRepository configurationParameterRepository,
    IWarningCollector warnings,
    ILogger<ValidateTrustYieldsDistributionService> logger)
    : IValidateTrustYieldsDistributionService
{
    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        using var _ = logger.BeginScope(new System.Collections.Generic.Dictionary<string, object>
        {
            ["Service"] = "ValidateTrustYieldsDistributionService",
            ["PortfolioId"] = portfolioId,
            ["ClosingDate"] = closingDate
        });
        const string svc = "[ValidateTrustYieldsDistributionService]";

        logger.LogInformation("{Svc} Inicio de validación de distribución de rendimientos.", svc);

        var now = DateTime.UtcNow;

        // Publicar evento de paso
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", now, cancellationToken);
        logger.LogInformation("{Svc} Paso de control de tiempo actualizado: NowUtc={NowUtc}", svc, now);

        var yield = await yieldRepository.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
        if (yield is null)
        {
            logger.LogWarning("{Svc} No se encontró información de rendimientos para {ClosingDate}", svc, closingDate);
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Yield obtenido: Income={Income}, Expenses={Expenses}, Commissions={Commissions}, Costs={Costs}, YieldToCredit={YieldToCredit}",
            svc, yield.Income, yield.Expenses, yield.Commissions, yield.Costs, yield.YieldToCredit);

        var trustYields = await trustYieldRepository.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
        if (!trustYields.Any())
        {
            logger.LogWarning("{Svc} No existen rendimientos distribuidos para fideicomisos en {ClosingDate}", svc, closingDate);
            return Result.Failure(new Error("002", "No existen registros de rendimientos distribuidos para fideicomisos.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Registros de trust_yields encontrados: {Count}", svc, trustYields.Count);

        var expectedTotal = MoneyHelper.Round2(yield.YieldToCredit);
        var distributedTotal = trustYields
    .AsEnumerable()
    .Sum(t => MoneyHelper.Round2(t.YieldAmount));
        var difference = MoneyHelper.Round2(expectedTotal - distributedTotal);

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
            processDate: DateTime.UtcNow,
            isClosed: yield.IsClosed
        );

        await yieldRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("{Svc} Cambios persistidos en yields.", svc);

        if (difference != 0m)
        {
            var toleranceParam = await configurationParameterRepository.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, cancellationToken);
            if (toleranceParam?.Metadata is null)
                return Result.Failure(new Error("001", $"No se encontró metadata para 'YieldDifferenceTolerance'.", ErrorType.Failure));
            var tolerance = JsonDecimalHelper.ExtractDecimal(toleranceParam.Metadata, "valor", isPercentage: false);

            if (tolerance < 0m)
                return Result.Failure(new Error("001A", "Tolerancia inválida (< 0).", ErrorType.Failure));

            if (Math.Abs(difference) > tolerance)
            {
                warnings.Add(WarningCatalog.Adv003YieldDifference(difference, tolerance));
                logger.LogWarning("{Svc} Diferencia de rendimiento fuera de tolerancia: {Difference}. Tolerancia: {Tolerance}", svc, difference, tolerance);
            }
            logger.LogInformation("{Svc} Se detectó diferencia distinta de 0. Se generará ajuste.", svc);
            var nextClosingDate = closingDate.AddDays(1);
            logger.LogInformation("{Svc} Próxima fecha de cierre para ajuste: {NextClosingDate}", svc, nextClosingDate);
            var isIncome = difference > 0m;
            var uuid = isIncome
                    ? ConfigurationParameterUuids.Closing.YieldAdjustmentIncome
                    : ConfigurationParameterUuids.Closing.YieldAdjustmentExpense;
            var adjustmentParam = await configurationParameterRepository.GetByUuidAsync(uuid, cancellationToken);
            if (adjustmentParam?.Metadata is null)
                return Result.Failure(new Error("001",$"No se encontró metadata para '{uuid}'.", ErrorType.Failure));

            var conceptId = JsonIntegerHelper.ExtractInt32(adjustmentParam.Metadata, "id", defaultValue: 0);
            var conceptName = JsonStringHelper.ExtractString(adjustmentParam.Metadata, "nombre", defaultValue: string.Empty);

            if (conceptId <= 0 || string.IsNullOrWhiteSpace(conceptName))
                return Result.Failure(new Error("002", $"Metadata inválida para '{uuid}': id/nombre requeridos.", ErrorType.Failure));

            var summary = new AutomaticConceptSummary(
                ConceptId: conceptId,
                ConceptName: conceptName,
                Nature: isIncome ? IncomeExpenseNature.Income : IncomeExpenseNature.Expense,
                Source: YieldsSources.AutomaticConcept,
                TotalAmount: Math.Abs(difference)
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

            await yieldDetailCreationService.CreateYieldDetailsAsync(buildResult, PersistenceMode.Transactional, cancellationToken);

            logger.LogInformation("{Svc} Ajuste generado y persistido: Difference={Difference}, NextClosingDate={NextClosingDate}", svc, difference, nextClosingDate);
        }
        else
        {
            logger.LogInformation("{Svc} Sin diferencias: no se genera ajuste.", svc);
        }

        logger.LogInformation("{Svc} Validación de distribución finalizada correctamente.", svc);
        return Result.Success();
    }
}
