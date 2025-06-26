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

        var portfolioValidationResult = await portfolioValidator
            .EnsureExistsAsync(command.PortfolioId, cancellationToken);
        if (!portfolioValidationResult.IsSuccess)
            return Result.Failure<bool>(portfolioValidationResult.Error!);

        var conceptNames = command.ConceptAmounts.Keys.ToArray();
        var profitLossConcepts = await conceptRepository
            .FindByNamesAsync(conceptNames, cancellationToken);

        if (profitLossConcepts.Count != conceptNames.Length)
        {
            var missingConceptNames = conceptNames.Except(profitLossConcepts.Select(c => c.Concept));
            return Result.Failure<bool>(Error.Validation(
                "Concept.NotFound",
                $"No existen los conceptos: {string.Join(", ", missingConceptNames)}"));
        }

        var ruleContext = new
        {
            command.EffectiveDate,
            Concepts = profitLossConcepts.Select(c => new
            {
                c.ProfitLossConceptId,
                c.Concept,
                c.Nature,
                c.AllowNegative,
                Amount = command.ConceptAmounts[c.Concept]
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
        var profitLossEntries = ruleContext.Concepts.Select(item =>
            ProfitLoss.Create(
                command.PortfolioId,
                processDateUtc,
                command.EffectiveDate,
                item.ProfitLossConceptId,
                item.Amount,
                "Externa").Value);

        profitLossRepository.InsertRange(profitLossEntries);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}