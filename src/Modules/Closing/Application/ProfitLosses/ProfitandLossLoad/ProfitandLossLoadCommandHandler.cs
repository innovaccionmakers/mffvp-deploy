using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Integrations.ProfitLosses.ProfitandLossLoad;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;

namespace Closing.Application.ProfitLosses.ProfitandLossLoad;

internal sealed class ProfitandLossLoadCommandHandler(
    IProfitLossConceptRepository conceptRepository,
    IProfitLossRepository profitLossRepository,
    IPortfolioValidator portfolioValidator,
    IInternalRuleEvaluator<ClosingModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ProfitandLossLoadCommand, bool>
{
    private const string WorkflowName = "Closing.ProfitLoss.UploadValidationV2";

    public async Task<Result<bool>> Handle(ProfitandLossLoadCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var portfolioDataResult = await portfolioValidator
            .GetPortfolioDataAsync(command.PortfolioId, cancellationToken);
        if (!portfolioDataResult.IsSuccess)
            return Result.Failure<bool>(portfolioDataResult.Error!);

        var portfolioData = portfolioDataResult.Value;

        var conceptNames = command.ConceptAmounts.Keys.ToArray();
        var profitLossConcepts = await conceptRepository
            .FindByNamesAsync(conceptNames, cancellationToken);

        var ruleContext = new
        {
            command.EffectiveDate,
            PortfolioCurrentDate = portfolioData.CurrentDate,
            RequestedConceptNames = conceptNames,
            Concepts = profitLossConcepts.Select(c => new
            {
                c.ProfitLossConceptId,
                c.Concept,
                c.Nature,
                c.AllowNegative,
                Amount = command.ConceptAmounts[c.Concept],
                IsIncome = c.Nature == IncomeExpenseNature.Income,
                IsExpense = c.Nature == IncomeExpenseNature.Expense
            }).ToArray()
        };

        var (isValid, _, validationErrors) = await ruleEvaluator
            .EvaluateAsync(WorkflowName, ruleContext, cancellationToken);
        if (!isValid)
        {
            var firstError = validationErrors.First();
            return Result.Failure<bool>(Error.Validation(firstError.Code, firstError.Message));
        }

        await profitLossRepository
            .DeleteByPortfolioAndDateAsync(command.PortfolioId, command.EffectiveDate, cancellationToken);

        var processDateUtc = DateTime.UtcNow;
        var effectiveDateUtc =
            command.EffectiveDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(command.EffectiveDate, DateTimeKind.Utc)
                : command.EffectiveDate.ToUniversalTime();
        var profitLossEntries = ruleContext.Concepts.Select(item =>
            ProfitLoss.Create(
                command.PortfolioId,
                processDateUtc,
                effectiveDateUtc,
                item.ProfitLossConceptId,
                item.Amount,
                "Externa").Value).ToArray();

        profitLossRepository.InsertRange(profitLossEntries);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(true, "La operacion se realizo exitosamente.");
    }
}