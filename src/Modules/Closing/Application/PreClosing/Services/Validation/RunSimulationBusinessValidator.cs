
using Closing.Application.Abstractions;
using Closing.Application.Abstractions.External;
using Closing.Application.Abstractions.External.Operations.SubtransactionTypes;
using Closing.Application.Abstractions.External.Products.Commissions;
using Closing.Application.PreClosing.Services.Commission.Constants;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Application.PreClosing.Services.Validation.Context;
using Closing.Application.PreClosing.Services.Validation.Dto;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.ProfitLosses;
using Closing.Domain.Rules;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Application.Helpers.Rules;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;

namespace Closing.Application.PreClosing.Services.Validation;

public class RunSimulationBusinessValidator(
        IRuleEvaluator<ClosingModuleMarker> ruleEvaluator,
        IPortfolioValidator portfolioValidator,
        IPortfolioValuationRepository portfolioValuationRepository,
        IProfitLossRepository profitLossRepository,
        ICommissionLocator commissionLocator,
        IMovementsConsolidationService movementsConsolidationService,
        IClientOperationRepository clientOperationRepository,
        ISubtransactionTypesLocator subtransactionTypesLocator,
        IConfigurationParameterRepository configurationParameterRepository
    ) : IBusinessValidator<RunSimulationCommand>
{
    private readonly GenericWorkflowValidator<ClosingModuleMarker> _workflowValidator =
               new(new ExternalRuleEvaluatorAdapter<ClosingModuleMarker>(ruleEvaluator));
    public async Task<Result> ValidateAsync(RunSimulationCommand command, CancellationToken ct)
    {
        // 1. Portfolio
        var portfolioDataResult = await portfolioValidator.GetPortfolioDataAsync(command.PortfolioId, ct);
        if (!portfolioDataResult.IsSuccess)
            return Result.Failure(portfolioDataResult.Error!);

        var portfolioData = portfolioDataResult.Value;

        // 2. (Opcional) ¿Ya existe cierre generado para esta fecha? -> bool
        var existsClosingGenerated = await portfolioValuationRepository
            .ValuationExistsAsync(command.PortfolioId, command.ClosingDate.Date, ct);

        // 3. Comisiones administrativas
        var commissionResult = await commissionLocator.GetActiveCommissionsAsync(command.PortfolioId, ct);
        if (!commissionResult.IsSuccess)
            return Result.Failure(commissionResult.Error!);

        var (adminCount, adminIsNumber, adminBetween0And100) = EvaluateAdminCommission(commissionResult.Value);

        // 4. Estado primer día de cierre (incluye InitialFundUnitValue, P&L, etc.)
        var firstDayState = await EvaluateFirstDayStateAsync(command, portfolioData, ct);
        if (firstDayState.Failure.IsFailure)
            return Result.Failure(firstDayState.Failure.Error!); // error técnico, abortar

        // 5. Construir contexto fluido
        var ctx = new PreclosingValidationContextBuilder()
            .WithClosingDate(DateOnly.FromDateTime(command.ClosingDate))
            .WithCurrentDate(DateOnly.FromDateTime(portfolioData.CurrentDate))
            .WithFirstDayState(firstDayState) // incluye IsFirstClosingDay, P&L, Tesorería, ClientOps, IFUV flags
            .WithAdminCommission(adminCount, adminIsNumber, adminBetween0And100) // opcional, pero tenemos datos
            .WithClosingGenerated(existsClosingGenerated) // opcional, pero tenemos datos
            .Build();

        // 6. Preparar lista de workflows
        var workflows = new List<string>
            {
                WorkflowNames.Preclosing.Simulation.GeneralBlockingValidations
            };
        if (firstDayState.IsFirstDay)
        {
            workflows.Add(WorkflowNames.Preclosing.Simulation.FirstDayBlockingValidations);
        }

        // 7. Ejecutar
        var wfResult = await _workflowValidator.EvaluateManyAsync(
            workflows,
            ctx,
            ErrorSelection.First, // o ErrorSelection.All si quieres acumular
            WorkflowEvaluationMode.ShortCircuitOnFailure, // recomendado para bloqueantes
            ct);

        if (!wfResult.IsValid)
        {
            // Mapear primer error surfaced (por config) a tu dominio Result
            var e = wfResult.Errors[0];
            return Result.Failure(Error.Validation(e.Code, e.Message));
        }

        return Result.Success();
    }
    private (int count, bool isNumber, bool between0And100) EvaluateAdminCommission(IEnumerable<CommissionsByPortfolioRemoteResponse> commissions)
    {
        var admin = commissions
            .Where(c => c.Concept.Equals(CommissionConcepts.Administrative, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var value = admin.Select(c => c.CalculationRule).FirstOrDefault();
        var isNumber = decimal.TryParse(value, out var pct);
        var inRange = isNumber && pct >= 0 && pct <= 100;

        return (admin.Count, isNumber, inRange);
    }

    private async Task<FirstDayStateResult> EvaluateFirstDayStateAsync(
    RunSimulationCommand command,
    PortfolioData portfolioData,
    CancellationToken ct)
    {
        // 1. ¿Es primer día de cierre?
        var isFirstClosingDay = !await portfolioValuationRepository
            .ValuationExistsAsync(command.PortfolioId, portfolioData.CurrentDate.Date, ct);

        // Inicializar flags por defecto
        bool hasPandL = false;
        bool hasTreasury = false;
        bool hasClientOps = false;
        bool hasInitialFundUnitValue = false;
        bool isInitialFundUnitValueValid = false;
        decimal? initialFundUnitValue = null;

        // 2. Solo si es primer día, cargar/validar todo lo que aplica a "día 1"
        if (isFirstClosingDay)
        {
            // --- Param: InitialFundUnitValue ---
            var param = await configurationParameterRepository
                .GetByUuidAsync(ConfigurationParameterUuids.Closing.InitialFundUnitValue, ct);

            if (param is not null && !string.IsNullOrWhiteSpace(param.Metadata.ToString()))
            {
                hasInitialFundUnitValue = true;

                // Intenta extraer número
                decimal? parsed = JsonDecimalHelper.ExtractDecimal(param.Metadata, "Valor");
                if (parsed.HasValue && parsed.Value > 0m)
                {
                    isInitialFundUnitValueValid = true;
                    initialFundUnitValue = parsed.Value;
                }
            }

            // --- Subtransaction types & client ops ---
            var stResult = await subtransactionTypesLocator.GetAllSubtransactionTypesAsync(ct);
            if (!stResult.IsSuccess)
            {
                // Error técnico: no podemos ni saber si hay client ops.
                return new FirstDayStateResult(
                    true, false, false, false,
                    hasInitialFundUnitValue, isInitialFundUnitValueValid, initialFundUnitValue,
                    Result.Failure(stResult.Error!));
            }

            var incomeSubtypes = stResult.Value.Where(st => st.Nature == IncomeEgressNature.Income).ToList();

            // NOTA: Que no haya incomeSubtypes es una condición funcional; lo modelamos vía hasClientOps=false
            // y que tu RulesEngine tenga una regla "Debe existir al menos un tipo de ingreso".
            // Si prefieres tratarlo como error técnico, cambia aquí.
            if (incomeSubtypes.Count > 0)
            {
                foreach (var item in incomeSubtypes)
                {
                    if (await clientOperationRepository.ClientOperationsExistsAsync(
                            command.PortfolioId, command.ClosingDate.Date, item.SubtransactionTypeId, ct))
                    {
                        hasClientOps = true;
                        break;
                    }
                }
            }

            // --- P&L y Tesorería ---
            hasPandL = await profitLossRepository.PandLExistsAsync(
                command.PortfolioId, command.ClosingDate.Date, ct);

            hasTreasury = await movementsConsolidationService.HasTreasuryMovementsAsync(
                command.PortfolioId, command.ClosingDate.Date, ct);
        }

        // 3. Retornar estado completo (Failure.Success para camino feliz o cuando solo hay "errores funcionales")
        return new FirstDayStateResult(
            isFirstClosingDay,
            hasPandL,
            hasTreasury,
            hasClientOps,
            hasInitialFundUnitValue,
            isInitialFundUnitValueValid,
            initialFundUnitValue,
            Result.Success());
    }


}