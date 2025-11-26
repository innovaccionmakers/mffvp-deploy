using Accounting.Integrations.PassiveTransaction.CreatePassiveTransaction;
using Accounting.Integrations.PassiveTransaction.DeletePassiveTransaction;
using Accounting.Integrations.PassiveTransaction.UpdatePassiveTransaction;
using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class PassiveTransactionMutations(
        ISender mediator) : IPassiveTransactionMutations
    {
        public async Task<GraphqlResult> CreatePassiveTransactionAsync(CreatePassiveTransactionInput input, IValidator<CreatePassiveTransactionInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult();
            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);

                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new CreatePassiveTransactionCommand(
                    input.PortfolioId,
                    input.TypeOperationsId,
                    input.DebitAccount,
                    input.CreditAccount,
                    input.ContraCreditAccount,
                    input.ContraDebitAccount
                );

                var commandResult = await mediator.Send(command, cancellationToken);

                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }

                result.SetSuccess("Genial!, Se ha creado la configuración contable exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult> DeletePassiveTransactionAsync(DeletePassiveTransactionInput input, IValidator<DeletePassiveTransactionInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult();
            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);
                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new DeletePassiveTransactionCommand(
                    input.PortfolioId,
                    input.TypeOperationsId
                );

                var commandResult = await mediator.Send(command, cancellationToken);
                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }
                result.SetSuccess("Genial!, Se ha eliminado la configuración contable exitosamente");

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }        
        }

        public async Task<GraphqlResult> UpdatePassiveTransactionAsync(UpdatePassiveTransactionInput input, IValidator<UpdatePassiveTransactionInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult();
            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);
                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new UpdatePassiveTransactionCommand(
                    input.PortfolioId,
                    input.TypeOperationsId,
                    input.DebitAccount,
                    input.CreditAccount,
                    input.ContraCreditAccount,
                    input.ContraDebitAccount
                );

                var commandResult = await mediator.Send(command, cancellationToken);
                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }
                result.SetSuccess("Genial!, Se ha actualizado la configuración contable exitosamente");

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }
    }
}
