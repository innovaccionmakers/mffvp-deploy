namespace Treasury.Application.TreasuryMovements.Commands;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using System.Threading;
using System.Threading.Tasks;
using Treasury.Application.Abstractions;
using Treasury.Application.Abstractions.Data;
using Treasury.Application.Abstractions.External;
using Treasury.Domain.BankAccounts;
using Treasury.Domain.Issuers;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Domain.TreasuryMovements;
using Treasury.Integrations.TreasuryConcepts.Response;
using Treasury.Integrations.TreasuryMovements.Commands;

internal class CreateTreasuryMovementCommandHandler(ITreasuryMovementRepository repository,
                                                    ITreasuryConceptRepository treasuryConceptRepository,
                                                    IUnitOfWork unitOfWork,
                                                    IPortfolioLocator portfolioLocator,
                                                    IPortfolioValuationLocator portfolioValuationLocator,
                                                    IBankAccountRepository bankAccountRepository,
                                                    IIssuerRepository issuerRepository,
                                                    IInternalRuleEvaluator<TreasuryModuleMarker> ruleEvaluator) : ICommandHandler<CreateTreasuryMovementCommand, TreasuryMovementResponse>
{
    private const string RequiredFieldsWorkflow = "Treasury.CreateTreasuryMovement.RequiredFields";
    private const string TreasuryMovementValidationWorkflow = "Treasury.CreateTreasuryMovement.Validation";


    public async Task<Result<TreasuryMovementResponse>> Handle(CreateTreasuryMovementCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequiredFieldsAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<TreasuryMovementResponse>(validationResult.Error);
        }

        var treasuryMovementsResult = await ValidateBusinessRulesAndCreateMovementsAsync(request, cancellationToken);
        if (treasuryMovementsResult.IsFailure)
        {
            return Result.Failure<TreasuryMovementResponse>(treasuryMovementsResult.Error);
        }

        return await SaveMovements(treasuryMovementsResult.Value, cancellationToken);
    }

    private async Task<Result> ValidateRequiredFieldsAsync(CreateTreasuryMovementCommand request, CancellationToken cancellationToken)
    {
        if (!request.Concepts.Any())
        {
            return Result.Failure(Error.Validation("TreasuryMovement.NoConcepts", "Debe especificar al menos un concepto"));
        }

        foreach (var concept in request.Concepts)
        {
            var requiredContext = new
            {
                request.PortfolioId,
                request.ClosingDate,
                concept.TreasuryConceptId,
                concept.Value,
                concept.BankAccountId,
                concept.CounterpartyId,
            };

            var (requiredOk, _, requiredErrors) = await ruleEvaluator.EvaluateAsync(RequiredFieldsWorkflow, requiredContext, cancellationToken);
            if (!requiredOk)
            {
                var first = requiredErrors.First();
                return Result.Failure(Error.Validation(first.Code, first.Message));
            }
        }

        return Result.Success();
    }

    private async Task<Result<List<TreasuryMovement>>> ValidateBusinessRulesAndCreateMovementsAsync(CreateTreasuryMovementCommand request, CancellationToken cancellationToken)
    {
        var portfolioRes = await portfolioLocator.FindByPortfolioIdAsync(request.PortfolioId, cancellationToken);
        if (portfolioRes.IsFailure)
        {
            return Result.Failure<List<TreasuryMovement>>(portfolioRes.Error);
        }

        var existPortfolioValuation = await portfolioValuationLocator.CheckPortfolioValuationExists(portfolioRes.Value.PortfolioId, cancellationToken);
        var isClosingDateValid = request.ClosingDate.Date == portfolioRes.Value.CurrentDate.Date.AddDays(1);

        var treasuryMovements = new List<TreasuryMovement>();

        foreach (var concept in request.Concepts)
        {
            var treasuryConcept = await treasuryConceptRepository.GetByIdAsync(concept.TreasuryConceptId, cancellationToken);
            var bankAccount = await bankAccountRepository.GetByIdAsync(concept.BankAccountId, cancellationToken);
            var counterparty = await issuerRepository.GetByIdAsync(concept.CounterpartyId, cancellationToken);

            var validationContext = new
            {
                TreasuryConceptExists = treasuryConcept,
                CounterpartyExists = counterparty,
                BankAccountExists = bankAccount,
                PortfolioExists = portfolioRes,
                AllowsNegative = treasuryConcept?.AllowsNegative ?? false,
                concept.Value,
                PortfolioValuationExists = existPortfolioValuation.Value,
                ClosingDateIsValid = isClosingDateValid
            };

            var (rulesOk, _, ruleErrors) = await ruleEvaluator
                .EvaluateAsync(TreasuryMovementValidationWorkflow,
                    validationContext,
                    cancellationToken);

            if (!rulesOk)
            {
                var first = ruleErrors.First();
                return Result.Failure<List<TreasuryMovement>>(Error.Validation(first.Code, first.Message));
            }
           
            var treasuryMovement = TreasuryMovement.Create(
                request.PortfolioId,
                DateTime.UtcNow,
                request.ClosingDate,
                concept.TreasuryConceptId,
                concept.Value,
                concept.BankAccountId,
                bankAccount?.IssuerId ?? 0,
                concept.CounterpartyId
            );

            if (treasuryMovement.IsFailure)
            {
                return Result.Failure<List<TreasuryMovement>>(treasuryMovement.Error);
            }

            treasuryMovements.Add(treasuryMovement.Value);
        }

        return Result.Success(treasuryMovements);
    }


    private async Task<Result<TreasuryMovementResponse>> SaveMovements(List<TreasuryMovement> treasuryMovements, CancellationToken cancellationToken)
    {
        var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await repository.AddRangeAsync(treasuryMovements, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var response = new TreasuryMovementResponse(
                treasuryMovements.Select(m => m.Id).ToList()
            );

            return Result.Success(response);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

}
