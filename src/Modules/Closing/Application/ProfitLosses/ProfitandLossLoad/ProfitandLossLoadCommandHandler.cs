using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Application.Rules;
using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Integrations.ProfitLosses.ProfitandLossLoad;

namespace Closing.Application.ProfitLosses.ProfitandLossLoad;

internal sealed class ProfitandLossLoadCommandHandler(
    IProfitLossConceptRepository conceptRepository,
    IProfitLossRepository profitLossRepository,
    IPortfolioValidator portfolioValidator,
    IRuleEvaluator<ClosingModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork
) : ICommandHandler<ProfitandLossLoadCommand, bool>
{
    private const string Workflow = "Closing.ProfitLoss.UploadValidation";

    public async Task<Result<bool>> Handle(ProfitandLossLoadCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        var portfolioResult = await portfolioValidator.EnsureExistsAsync(request.PortfolioId, cancellationToken);
        if (!portfolioResult.IsSuccess)
        {
            return Result.Failure<bool>(portfolioResult.Error!);
        }

        var grossConcept = await conceptRepository.FindByNameAsync("Rendimientos Brutos", cancellationToken);
        var expenseConcept = await conceptRepository.FindByNameAsync("Gastos", cancellationToken);

        var validationContext = new
        {
            AllowNegative = expenseConcept?.AllowNegative ?? false,
            Expenses = request.Expenses,
            EffectiveDate = request.EffectiveDate,
            GrossConceptExists = grossConcept is not null,
            ExpenseConceptExists = expenseConcept is not null
        };
        
        var (ok, _, errors) = await ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

        if (!ok)
        {
            var first = errors.First();
            return Result.Failure<bool>(Error.Validation(first.Code, first.Message));
        }

        await profitLossRepository.DeleteByPortfolioAndDateAsync(request.PortfolioId, request.EffectiveDate, cancellationToken);

        var now = DateTime.UtcNow;
        
        var gross = ProfitLoss.Create(
            request.PortfolioId,
            now,
            request.EffectiveDate,
            grossConcept!.ProfitLossConceptId,
            request.GrossReturns,
            "Externa").Value;

        var expense = ProfitLoss.Create(
            request.PortfolioId,
            now,
            request.EffectiveDate,
            expenseConcept!.ProfitLossConceptId,
            request.Expenses,
            "Externa").Value;


        profitLossRepository.InsertRange(new[] { gross, expense });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}