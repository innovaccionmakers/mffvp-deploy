using Closing.Application.Abstractions;
using Closing.Application.Closing.Services.Validation.Context;
using Closing.Application.Closing.Services.Validation.Dto;
using Closing.Application.Closing.Services.Validation.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.Rules;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Application.Helpers.Rules;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using System.Text.Json;


namespace Closing.Application.Closing.Services.Validation;

/// Si NO es primer día: deben existir los parámetros requeridos y,
/// para los numéricos, que metadata.valor sea numérico > 0.
public sealed class PrepareClosingBusinessValidator(
    IRuleEvaluator<ClosingModuleMarker> ruleEvaluator,
    IConfigurationParameterRepository configurationParameterRepository
) : IPrepareClosingBusinessValidator
{
    private readonly GenericWorkflowValidator<ClosingModuleMarker> workflowValidator =
        new(new ExternalRuleEvaluatorAdapter<ClosingModuleMarker>(ruleEvaluator));

    public async Task<Result> ValidateAsync(
        PrepareClosingCommand command,
        bool isFirstClosingDay,
        CancellationToken cancellationToken = default)
    {
        bool hasWithholdingPercentage = false;
        bool withholdingPercentageIsValid = false;

        bool hasYieldTolerance = false;
        bool yieldToleranceIsValid = false;

        bool hasAutomaticConceptIncome = false;
        bool hasAutomaticConceptExpense = false;

        if (!isFirstClosingDay)
        {
            var retentionParam = await configurationParameterRepository
                .GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldRetentionPercentage, cancellationToken);
            var toleranceParam = await configurationParameterRepository
                .GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, cancellationToken);
            var incomeParam = await configurationParameterRepository
                .GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome, cancellationToken);
            var expenseParam = await configurationParameterRepository
                .GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentExpense, cancellationToken);

            hasWithholdingPercentage = retentionParam is not null;
            hasYieldTolerance = toleranceParam is not null;
            hasAutomaticConceptIncome = incomeParam is not null;
            hasAutomaticConceptExpense = expenseParam is not null;

            if (retentionParam is not null)
                withholdingPercentageIsValid = TryReadPositiveNumberFromMetadata(retentionParam.Metadata, out _);

            if (toleranceParam is not null)
                yieldToleranceIsValid = TryReadPositiveNumberFromMetadata(toleranceParam.Metadata, out _);
        }

        var context = new PrepareClosingValidationContextBuilder()
            .WithClosingDate(command.ClosingDate)
            .WithIsFirstClosingDay(isFirstClosingDay)
            .WithConfigFlags(new ConfigFlags(
                hasWithholdingPercentage,
                withholdingPercentageIsValid,
                hasYieldTolerance,
                yieldToleranceIsValid,
                hasAutomaticConceptIncome,
                hasAutomaticConceptExpense))
            .Build();

        var workflows = new List<string>
        {
            WorkflowNames.RunClosing.SecondDayBlockingValidations
        };

        var evaluation = await workflowValidator.EvaluateManyAsync(
            workflows,
            context,
            ErrorSelection.First,
            WorkflowEvaluationMode.ShortCircuitOnFailure,
            cancellationToken);

        if (!evaluation.IsValid)
        {
            var e = evaluation.Errors[0];
            return Result.Failure(Error.Validation(e.Code, e.Message));
        }

        return Result.Success();
    }

    private static bool TryReadPositiveNumberFromMetadata(JsonDocument metadata, out decimal value)
    {
        value = 0m;
        if (metadata is null) return false;
        try
        {
            decimal? parsed = JsonDecimalHelper.ExtractDecimal(metadata, "valor");
            if (parsed.HasValue && parsed.Value > 0m)
            {
                value = parsed.Value;
                return true;
            }
        }
        catch { return false; }

        return false;
    }
}
