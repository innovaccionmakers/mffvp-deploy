
using Closing.Application.Abstractions;
using Closing.Application.Abstractions.External;
using Closing.Application.Abstractions.External.Products.Commissions;
using Closing.Domain.Constants;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.ProfitLosses;
using Closing.Domain.Rules;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Validation;

public class RunSimulationBusinessValidator(
        IInternalRuleEvaluator<ClosingModuleMarker> ruleEvaluator,
        IPortfolioValidator portfolioValidator,
        IPortfolioValuationRepository portfolioValuationRepository,
        IProfitLossRepository profitLossRepository,
        ICommissionLocator commissionLocator
    ) : IBusinessValidator<RunSimulationCommand>
{
    public async Task<Result> ValidateAsync(RunSimulationCommand command, CancellationToken ct)
    {
        var portfolioDataResult = await portfolioValidator.GetPortfolioDataAsync(command.PortfolioId, ct);
        if (!portfolioDataResult.IsSuccess)
            return Result.Failure(portfolioDataResult.Error!);

        var portfolioData = portfolioDataResult.Value;

        //En valoracion_portafolio si para esa FECHA ACTUAL DEL FONDO hay datos, si no hay datos quiere decir es el primer dia 
        var isFirstClosingDay = !await portfolioValuationRepository.ValuationExistsAsync(command.PortfolioId, portfolioData.CurrentDate.Date, ct);

        var hasPandL = await profitLossRepository.PandLExistsAsync(command.PortfolioId, command.ClosingDate.Date, ct);

        var commissionResult = await commissionLocator.GetActiveCommissionsAsync(command.PortfolioId, ct);
        if (!commissionResult.IsSuccess)
            return Result.Failure(commissionResult.Error!);
        var commissions = commissionResult.Value;
        var adminCommissions = commissions
             .Where(c => c.Concept.Equals(CommissionConcepts.Administrative, StringComparison.OrdinalIgnoreCase))
             .ToList();
        var adminCommissionCount = adminCommissions.Count;
        var adminCommissionValue = adminCommissions
            .Select(c => c.CalculationRule)
            .FirstOrDefault();

        var ruleContext = new
        {
            ClosingDate = command.ClosingDate.Date,
            CurrentDate = portfolioData.CurrentDate.Date,
            IsFirstClosingDay = isFirstClosingDay,
            HasPandL = hasPandL,
            AdminCommissionCount = adminCommissionCount
        };

        var (isValid, _, validationErrors) = await ruleEvaluator
            .EvaluateAsync(WorkflowNames.PreclosingValidations, ruleContext, ct);

        if (!isValid)
        {
            var firstError = validationErrors.First();
            return Result.Failure(Error.Validation(firstError.Code, firstError.Message));
        }

        // Paso 2: Validación - ¿Es primer día de cierre?
        //var isFirstClosingDay = portfolioData.CurrentDate == null;

        //if (isFirstClosingDay)
        //{
        //    // Validar que no haya PyG cargado
        //    var profitSummary = await profitService.GetProfitAndLossSummaryAsync(command.PortfolioId, command.ClosingDate);
        //    if (profitSummary.Any())
        //    {
        //        return Result.Failure(Error.Validation("PYG_EXISTS", "No debe existir PYG en el primer día de cierre."));
        //    }

        //    // Validar que existan operaciones
        //    var treasurySummary = await movementService.GetMovementsSummaryAsync(command.PortfolioId, command.ClosingDate, ct);
        //    if (!treasurySummary.Any())
        //    {
        //        return Result.Failure(Error.Validation("NO_OPERATIONS", "Debe haber operaciones de entrada el primer día de cierre."));
        //    }
        //}

        //// Paso 3: Validación de comisiones
        //var commissionSummary = await commissionService.CalculateAsync(command.PortfolioId, command.ClosingDate, ct);
        //if (!commissionSummary.Any())
        //{
        //    return Result.Failure(Error.Validation("NO_COMMISSION", "Debe existir al menos un registro de comisión para la simulación."));
        //}

        //if (commissionSummary.Count > 1)
        //{
        //    return Result.Failure(Error.Validation("MULTIPLE_COMMISSIONS", "Solo debe existir una comisión para esta simulación."));
        //}

        //var commissionValue = commissionSummary.First().CommissionPercentage;
        //if (commissionValue <= 0 || commissionValue > 100)
        //{
        //    return Result.Failure(Error.Validation("INVALID_COMMISSION_RANGE", "La comisión debe estar entre 0 y 100."));
        //}

        return Result.Success();
    }
}