using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Treasury.Application.Abstractions;
using Treasury.Application.Abstractions.Data;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Integrations.TreasuryConcepts.Commands;
using Treasury.Integrations.TreasuryConcepts.Response;

namespace Treasury.Application.TreasuryConcepts.Commands;

internal class UpdateTreasuryConceptCommandHandler(ITreasuryConceptRepository repository,
                                                   IUnitOfWork unitOfWork,
                                                   IInternalRuleEvaluator<TreasuryModuleMarker> ruleEvaluator) : ICommandHandler<UpdateTreasuryConceptCommand, TreasuryConceptResponse>
{
    private const string RequiredFieldsWorkflow = "Treasury.SaveTreasuryConcept.RequiredFields";
    private const string ValidationWorkflow = "Treasury.UpdateTreasuryConcept.Validation";

    public async Task<Result<TreasuryConceptResponse>> Handle(UpdateTreasuryConceptCommand request, CancellationToken cancellationToken)
    {
        var requiredContext = new
        {
            request.Concept,
            request.Nature,
            request.AllowsNegative,
            request.AllowsExpense,
            request.RequiresBankAccount,
            request.RequiresCounterparty,
        };

        var (requiredOk, _, requiredErrors) = await ruleEvaluator.EvaluateAsync(RequiredFieldsWorkflow, requiredContext, cancellationToken);
        if (!requiredOk)
        {
            var first = requiredErrors.First();
            return Result.Failure<TreasuryConceptResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var existingConcept = await repository.GetByIdAsync(request.Id, cancellationToken);

        var validationContext = new
        {
            ExistingConcept = existingConcept
        };

        var (validationOk, _, validationErrors) = await ruleEvaluator.EvaluateAsync(ValidationWorkflow, validationContext, cancellationToken);
        if (!validationOk)
        {
            var first = validationErrors.First();
            return Result.Failure<TreasuryConceptResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var updateResult = existingConcept.Update(
            request.Concept,
            request.Nature,
            request.AllowsNegative,
            request.AllowsExpense,
            request.RequiresBankAccount,
            request.RequiresCounterparty,
            request.Observations ?? string.Empty
        );

        if (updateResult.IsFailure)
        {
            return Result.Failure<TreasuryConceptResponse>(updateResult.Error);
        }

        var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        await repository.UpdateAsync(existingConcept, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var response = new TreasuryConceptResponse(existingConcept.Id);
        return Result.Success(response);
    }
}