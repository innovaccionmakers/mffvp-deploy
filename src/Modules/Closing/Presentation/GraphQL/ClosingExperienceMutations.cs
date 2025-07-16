using Closing.Integrations.ProfitLosses.ProfitandLossLoad;
using Closing.Presentation.GraphQL.DTOs;
using Closing.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using Common.SharedKernel.Domain;
using FluentValidation;
using MediatR;

namespace Closing.Presentation.GraphQL;

public class ClosingExperienceMutations(IMediator mediator) : IClosingExperienceMutations
{
    public async Task<GraphqlMutationResult<LoadProfitLossResult>> LoadProfitLossAsync(
        LoadProfitLossInput input,
        IValidator<LoadProfitLossInput> validator,
        CancellationToken cancellationToken = default
    )
    {
        var result = new GraphqlMutationResult<LoadProfitLossResult>();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new ProfitandLossLoadCommand(
                input.PortfolioId,
                input.EffectiveDate,
                input.ConceptAmounts
            );

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            result.SetSuccess(new LoadProfitLossResult(commandResult.Value), "Carga de Profit & Loss realizada correctamente");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }

    }
}