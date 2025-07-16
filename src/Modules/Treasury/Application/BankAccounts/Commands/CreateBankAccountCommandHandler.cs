using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Treasury.Application.Abstractions;
using Treasury.Application.Abstractions.Data;
using Treasury.Domain.BankAccounts;
using Treasury.Integrations.BankAccounts.Commands;
using Treasury.Integrations.BankAccounts.Response;

namespace Treasury.Application.BankAccounts.Commands;

internal class CreateBankAccountCommandHandler(IBankAccountRepository repository,
                                               IUnitOfWork unitOfWork, IInternalRuleEvaluator<TreasuryModuleMarker> ruleEvaluator) : ICommandHandler<CreateBankAccountCommand, BankAccountResponse>
{
    private const string RequiredFieldsWorkflow = "Treasury.CreateBankAccount.RequiredFields";
    private const string BankAccountCreationValidationWorkflow = "Treasury.CreateBankAccount.Validation";
    public async Task<Result<BankAccountResponse>> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var requiredContext = new
        {
            request.PortfolioId,
            request.IssuerId,
            request.Issuer,
            request.AccountNumber,
            request.AccountType,
        };

        var (requiredOk, _, requiredErrors) = await ruleEvaluator.EvaluateAsync(RequiredFieldsWorkflow, requiredContext, cancellationToken);

        if (!requiredOk)
        {
            var first = requiredErrors.First();
            return Result.Failure<BankAccountResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var existBankAccount = await repository.ExistsAsync(request.IssuerId, request.AccountNumber, request.AccountType, cancellationToken);


        var validationContext = new
        {
            BankAccountExists = existBankAccount,
        };

        var (rulesOk, _, ruleErrors) = await ruleEvaluator
            .EvaluateAsync(BankAccountCreationValidationWorkflow,
                validationContext,
                cancellationToken);

        if (!rulesOk)
        {
            var first = ruleErrors.First();
            return Result.Failure<BankAccountResponse>(
                Error.Validation(first.Code, first.Message));
        }


        var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var bankAccount = BankAccount.Create(
            request.PortfolioId,
            request.IssuerId,
            request.AccountNumber,
            request.AccountType,
            request.Observations ?? string.Empty,
            DateTime.UtcNow
        );

        if (bankAccount.IsFailure)
            return Result.Failure<BankAccountResponse>(bankAccount.Error!);

        await repository.AddAsync(bankAccount.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var response = new BankAccountResponse(bankAccount.Value.Id);

        return Result.Success(response);
    }
}

