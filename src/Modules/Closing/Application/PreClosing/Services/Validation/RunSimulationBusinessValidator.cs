
using Closing.Application.Abstractions;
using Closing.Application.Abstractions.External;
using Closing.Application.Abstractions.External.Operations.SubtransactionTypes;
using Closing.Application.Abstractions.External.Products.Commissions;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Domain.ClientOperations;
using Closing.Domain.Constants;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.ProfitLosses;
using Closing.Domain.Rules;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;

namespace Closing.Application.PreClosing.Services.Validation;

public class RunSimulationBusinessValidator(
        IInternalRuleEvaluator<ClosingModuleMarker> ruleEvaluator,
        IPortfolioValidator portfolioValidator,
        IPortfolioValuationRepository portfolioValuationRepository,
        IProfitLossRepository profitLossRepository,
        ICommissionLocator commissionLocator,
        IMovementsConsolidationService movementsConsolidationService,
        IClientOperationRepository clientOperationRepository,
        ISubtransactionTypesLocator subtransactionTypesLocator
    ) : IBusinessValidator<RunSimulationCommand>
{
    public async Task<Result> ValidateAsync(RunSimulationCommand command, CancellationToken ct)
    {
        var portfolioDataResult = await portfolioValidator.GetPortfolioDataAsync(command.PortfolioId, ct);
        if (!portfolioDataResult.IsSuccess)
            return Result.Failure(portfolioDataResult.Error!);

        var portfolioData = portfolioDataResult.Value;

        //En valoracion_portafolio si para esa FECHA ACTUAL DEL FONDO no hay datos quiere decir es el primer dia 
        var isFirstClosingDay = !await portfolioValuationRepository.ValuationExistsAsync(command.PortfolioId, portfolioData.CurrentDate.Date, ct);

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

        var adminCommissionValueIsNumber = decimal.TryParse(adminCommissionValue, out var adminCommissionPercentage);

        var adminCommissionBetween0And100 = adminCommissionValueIsNumber && adminCommissionPercentage >= 0 && adminCommissionPercentage <= 100;

        var transactionSubtypesResult = await subtransactionTypesLocator.GetAllSubtransactionTypesAsync(ct);
        if (!transactionSubtypesResult.IsSuccess)
            return Result.Failure(transactionSubtypesResult.Error!);

        var transactionSubtypes = transactionSubtypesResult.Value;

        var incomeTransactionSubtypes = transactionSubtypes.Where(st => st.Nature == IncomeEgressNature.Income).ToList();

        if (!incomeTransactionSubtypes.Any())
        {
            return Result.Failure(new Error("001","No se tienen transacciones de Ingreso configuradas", ErrorType.Validation));
        }

        bool hasClientOperations = false;
        foreach (var item in incomeTransactionSubtypes)
        {
            hasClientOperations = await clientOperationRepository.ClientOperationsExistsAsync(command.PortfolioId, command.ClosingDate.Date, item.SubtransactionTypeId, ct);
            if (hasClientOperations)
                break;
        }

        var hasPandL = await profitLossRepository.PandLExistsAsync(command.PortfolioId, command.ClosingDate.Date, ct);

        var hasTreasuryMovements = await movementsConsolidationService.HasTreasuryMovementsAsync(command.PortfolioId, command.ClosingDate.Date, ct);

        var ruleContext = new
        {
            ClosingDate = command.ClosingDate.Date,
            CurrentDate = portfolioData.CurrentDate.Date,
            IsFirstClosingDay = isFirstClosingDay,
            HasPandL = hasPandL,
            AdminCommissionCount = adminCommissionCount,
            HasTreasuryMovements = hasTreasuryMovements,
            HasClientOperations = hasClientOperations,
            AdminCommissionIsNumber = adminCommissionValueIsNumber,
            AdminCommissionBetween0And100 = adminCommissionBetween0And100
        };

        var (isValid, _, validationErrors) = await ruleEvaluator
            .EvaluateAsync(WorkflowNames.PreclosingValidations, ruleContext, ct);

        if (!isValid)
        {
            var firstError = validationErrors.First();
            return Result.Failure(Error.Validation(firstError.Code, firstError.Message));
        }

            return Result.Success();
    }
}