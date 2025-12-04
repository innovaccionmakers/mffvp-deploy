using Accounting.Integrations.Treasuries.CreateTreasury;
using Accounting.Integrations.Treasuries.DeleteTreasury;
using Accounting.Integrations.Treasuries.UpdateTreasury;
using Accounting.Presentation.GraphQL.Inputs.TreasuriesInput;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class TreasuriesExperienceMutations(ISender mediator) : ITreasuriesExperienceMutations
    {
        public async Task<GraphqlResult> CreateTreasuryAsync(CreateTreasuryInput input, IValidator<CreateTreasuryInput> validator, CancellationToken cancellationToken = default)
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

                var command = new CreateTreasuryCommand(
                    input.PortfolioId,
                    input.BankAccount,
                    input.DebitAccount,
                    input.CreditAccount
                );

                var commandResult = await mediator.Send(command, cancellationToken);

                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }

                result.SetSuccess("Genial!, Se ha creado la tesoreria exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult> DeleteTreasuryAsync(DeleteTreasuryInput input, IValidator<DeleteTreasuryInput> validator, CancellationToken cancellationToken = default)
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

                var command = new DeleteTreasuryCommand(
                    input.PortfolioId,
                    input.BankAccount
                );

                var commandResult = await mediator.Send(command, cancellationToken);
                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }
                result.SetSuccess("Genial!, Se ha eliminado la tesoreria exitosamente");

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult> UpdateTreasuryAsync(UpdateTreasuryInput input, IValidator<UpdateTreasuryInput> validator, CancellationToken cancellationToken = default)
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

                var command = new UpdateTreasuryCommand(
                    input.PortfolioId,
                    input.BankAccount,
                    input.DebitAccount,
                    input.CreditAccount
                );

                var commandResult = await mediator.Send(command, cancellationToken);
                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }
                result.SetSuccess("Genial!, Se ha actualizado la tesoreria exitosamente");

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
