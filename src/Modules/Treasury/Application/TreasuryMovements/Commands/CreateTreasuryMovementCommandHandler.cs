namespace Treasury.Application.TreasuryMovements.Commands;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using System.Threading;
using System.Threading.Tasks;
using Treasury.Application.Abstractions;
using Treasury.Application.Abstractions.Data;
using Treasury.Domain.TreasuryMovements;
using Treasury.Integrations.TreasuryConcepts.Response;
using Treasury.Integrations.TreasuryMovements.Commands;

internal class CreateTreasuryMovementCommandHandler(ITreasuryMovementRepository repository,
                                                    IUnitOfWork unitOfWork,
                                                    IInternalRuleEvaluator<TreasuryModuleMarker> ruleEvaluator) : ICommandHandler<CreateTreasuryMovementCommand, TreasuryMovementResponse>
{
    private const string RequiredFieldsWorkflow = "Treasury.CreateTreasuryMovement.RequiredFields";
    public async Task<Result<TreasuryMovementResponse>> Handle(CreateTreasuryMovementCommand request, CancellationToken cancellationToken)
    {
        var requiredContext = new
        {
            request.PortfolioId,
            request.ClosingDate,
            request.TreasuryConceptId,
            request.Value,
            request.ProcessDate,
            request.BankAccountId,
            request.EntityId,
            request.CounterpartyId,            
        };

        var (requiredOk, _, requiredErrors) = await ruleEvaluator.EvaluateAsync(RequiredFieldsWorkflow, requiredContext, cancellationToken);
        if (!requiredOk)
        {
            var first = requiredErrors.First();
            return Result.Failure<TreasuryMovementResponse>(
                Error.Validation(first.Code, first.Message));
        }
        var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var treasuryMovement = TreasuryMovement.Create(
            request.PortfolioId,
            request.ClosingDate,
            request.ProcessDate,
            request.TreasuryConceptId,
            request.Value,
            request.BankAccountId,
            request.EntityId,
            request.CounterpartyId
        );

        if (treasuryMovement.IsFailure)
        {
            return Result.Failure<TreasuryMovementResponse>(
                treasuryMovement.Error);
        }

        await repository.AddAsync(treasuryMovement.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var response = new TreasuryMovementResponse(
            treasuryMovement.Value.Id            
        );

        return Result.Success(response);
    }
}
