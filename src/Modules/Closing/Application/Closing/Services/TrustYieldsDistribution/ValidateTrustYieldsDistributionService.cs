
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

        var now = DateTime.UtcNow;

        // Publicar evento de paso
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", now, cancellationToken);

        var yield = await yieldRepository.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
        if (yield is null)
        {
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }

        var trustYields = await trustYieldRepository.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
        if (!trustYields.Any())
        {
            return Result.Failure(new Error("002", "No existen registros de rendimientos distribuidos para fideicomisos.", ErrorType.Failure));
        }

        var expectedTotal = MoneyHelper.Round2(yield.YieldToCredit);
        var distributedTotal = trustYields
                            .AsEnumerable()
                            .Sum(t => MoneyHelper.Round2(t.YieldAmount));
        var difference = MoneyHelper.Round2(expectedTotal - distributedTotal);

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

            }

            var nextClosingDate = closingDate.AddDays(1);

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

            var parameters = new RunSimulationParameters(
                PortfolioId: portfolioId,
                ClosingDate: nextClosingDate,
                IsClosing: true
            );

            var buildResult = yieldDetailBuilderService.Build(
                new[] { summary }, parameters);

            await yieldDetailCreationService.CreateYieldDetailsAsync(buildResult, PersistenceMode.Transactional, cancellationToken);
        }
       
        return Result.Success();
    }
}
