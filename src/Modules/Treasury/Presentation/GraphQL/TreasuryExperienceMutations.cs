using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Treasury.Presentation.GraphQL.Input;
using Common.SharedKernel.Domain;
using Treasury.Integrations.BankAccounts.Commands;
using MediatR;

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
}
