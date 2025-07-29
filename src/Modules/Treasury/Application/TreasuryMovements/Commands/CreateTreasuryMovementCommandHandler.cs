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
        var requiredContext = new
        {
            request.PortfolioId,
            request.ClosingDate,
            request.TreasuryConceptId,
            request.Value,
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

        var treasuryConcept = await treasuryConceptRepository.GetByIdAsync(request.TreasuryConceptId, cancellationToken);
        var bankAccount = await bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
        var issuer = await issuerRepository.GetByIdAsync(request.EntityId, cancellationToken); 

        var portfolioRes = await portfolioLocator.FindByPortfolioIdAsync(request.PortfolioId, cancellationToken);
        if (portfolioRes.IsFailure)
        {
            return Result.Failure<TreasuryMovementResponse>(
                portfolioRes.Error);
        }
        var existPortfolioValuation = await portfolioValuationLocator.CheckPortfolioValuationExists(portfolioRes.Value.PortfolioId, cancellationToken);

        var isClosingDateValid = request.ClosingDate.Date == portfolioRes.Value.CurrentDate.Date.AddDays(1);

        var validationContext = new
        {
            TreasuryConceptExists = treasuryConcept,
            IssuerExists = issuer,
            BankAccountExists = bankAccount,
            PortfolioExists = portfolioRes,
            AllowsNegative = treasuryConcept?.AllowsNegative ?? false,
            request.Value,
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
            return Result.Failure<TreasuryMovementResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var treasuryMovement = TreasuryMovement.Create(
            request.PortfolioId,
            request.ClosingDate,
            DateTime.UtcNow,
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
