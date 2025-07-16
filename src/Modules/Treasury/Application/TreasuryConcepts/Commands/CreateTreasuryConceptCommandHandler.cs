using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Treasury.Application.Abstractions;
using Treasury.Application.Abstractions.Data;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Integrations.BankAccounts.Response;
using Treasury.Integrations.TreasuryConcepts.Commands;
using Treasury.Integrations.TreasuryConcepts.Response;

namespace Treasury.Application.TreasuryConcepts.Commands;

internal class CreateTreasuryConceptCommandHandler(ITreasuryConceptRepository repository, IUnitOfWork unitOfWork, IInternalRuleEvaluator<TreasuryModuleMarker> ruleEvaluator) : ICommandHandler<CreateTreasuryConceptCommand, TreasuryConceptResponse>
{   
    private const string RequiredFieldsWorkflow = "Treasury.CreateTreasuryConcept.RequiredFields";
    public async Task<Result<TreasuryConceptResponse>> Handle(CreateTreasuryConceptCommand request, CancellationToken cancellationToken)
    {
        var requiredContext = new
        {
            request.Concept,
            request.Nature,
            request.AllowsNegative,
            request.AllowsExpense,
            request.RequiresBankAccount,
            request.RequiresCounterparty,
            request.ProcessDate
        };

        var (requiredOk, _, requiredErrors) = await ruleEvaluator.EvaluateAsync(RequiredFieldsWorkflow, requiredContext, cancellationToken);
        if (!requiredOk)
        {
            var first = requiredErrors.First();
            return Result.Failure<TreasuryConceptResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var treasuryConcept = TreasuryConcept.Create(
            request.Concept,
            request.Nature,
            request.AllowsNegative,
            request.AllowsExpense,
            request.RequiresBankAccount,
            request.RequiresCounterparty,
            request.ProcessDate,
            request.Observations ?? string.Empty
        );

        if (treasuryConcept.IsFailure)
        {
            return Result.Failure<TreasuryConceptResponse>(
                treasuryConcept.Error);
        }

        await repository.AddAsync(treasuryConcept.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var response = new TreasuryConceptResponse(
            treasuryConcept.Value.Id          
        );

        return Result.Success(response);
    }
}
