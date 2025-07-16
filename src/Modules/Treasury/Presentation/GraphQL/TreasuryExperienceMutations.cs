using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;
using Treasury.Integrations.BankAccounts.Commands;
using Treasury.Integrations.TreasuryConcepts.Commands;
using Treasury.Integrations.TreasuryMovements.Commands;
using Treasury.Presentation.GraphQL.Input;

namespace Treasury.Presentation.GraphQL;

public class TreasuryExperienceMutations(IMediator mediator) : ITreasuryExperienceMutations
{
    public async Task<GraphqlMutationResult> AccountHandlerAsync(CreateAccountInput input, IValidator<CreateAccountInput> validator, CancellationToken cancellationToken = default)
    {
        var result = new GraphqlMutationResult();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);
            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new CreateBankAccountCommand(
                input.PortfolioId,
                input.IssuerId,
                input.Issuer,
                input.AccountNumber,
                input.AccountType,
                input.Observations
            );

            var commandResult = await mediator.Send(command, cancellationToken);
            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }
            result.SetSuccess("Genial!, Se ha creado la cuenta bancaria");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    public async Task<GraphqlMutationResult> TreasuryConfigHandlerAsync(CreateTreasuryOperationInput input, IValidator<CreateTreasuryOperationInput> validator, CancellationToken cancellationToken = default)
    {
        var result = new GraphqlMutationResult();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);
            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new CreateTreasuryConceptCommand(
                input.Concept,
                input.Nature,
                input.AllowsNegative,
                input.AllowsExpense,
                input.RequiresBankAccount,
                input.RequiresCounterparty,
                input.ProcessDate,
                input.Observations
            );

            var commandResult = await mediator.Send(command, cancellationToken);
            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }
            result.SetSuccess("Genial!, Se ha creado el concepto de tesorería");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    public async Task<GraphqlMutationResult> TreasuryOperationHandlerAsync(CreateTreasuryMovementInput input, IValidator<CreateTreasuryMovementInput> validator, CancellationToken cancellationToken = default)
    {
       var result = new GraphqlMutationResult();
        try 
        {
            var validationResult = RequestValidator.Validate(input, validator).GetAwaiter().GetResult();
            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }
            var command = new CreateTreasuryMovementCommand(
               input.PortfolioId,
                input.ClosingDate,
                input.TreasuryConceptId,
                input.Value,
                input.ProcessDate,
                input.BankAccountId,
                input.EntityId,
                input.CounterpartyId
            );
            var commandResult = await mediator.Send(command, cancellationToken);
            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }
            result.SetSuccess("Genial!, Se ha creado el movimiento de tesorería");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }
}
