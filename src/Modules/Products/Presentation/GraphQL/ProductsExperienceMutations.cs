using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using FluentValidation;

using MediatR;

using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.UpdateObjective;
using Products.Integrations.TechnicalSheets.Commands;
using Products.Presentation.DTOs;
using Products.Presentation.GraphQL.Input;

namespace Products.Presentation.GraphQL;

public class ProductsExperienceMutations(IMediator mediator) : IProductsExperienceMutations
{
    public async Task<GraphqlMutationResult<GoalMutationResult>> RegisterGoalAsync(CreateGoalInput input, IValidator<CreateGoalInput> validator, CancellationToken cancellationToken = default)
    {
        var result = new GraphqlMutationResult<GoalMutationResult>();
        try
        {

            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new CreateObjectiveCommand(
                input.IdType,
                input.Identification,
                input.AlternativeId,
                input.ObjectiveType,
                input.ObjectiveName,
                input.OpeningOffice,
                input.CurrentOffice,
                input.Commercial
            );

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var response = new GoalMutationResult(
                commandResult.Value.ObjectiveId ?? 0,
                "Objetivo",
                null
            );

            result.SetSuccess(response, "Genial!, Se ha creado el objetivo del Afiliado");
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }

    }

    public async Task<GraphqlMutationResult> SaveTechnicalSheetAsync(DateOnly closingDate, CancellationToken cancellationToken = default)
    {
        var result = new GraphqlMutationResult();
        try
        {
            var command =  new SaveTechnicalSheetCommand(closingDate);
            var commandResult = await mediator.Send(command, cancellationToken);
            
            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            result.SetSuccess("Proceso Ejecutado corretamente");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    public async Task<GraphqlMutationResult> UpdateGoalAsync(
        UpdateGoalInput input,
        IValidator<UpdateGoalInput> validator,
        CancellationToken cancellationToken = default)
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

            var command = new UpdateObjectiveCommand(
                input.ObjectiveId,
                input.ObjectiveType,
                input.ObjectiveName,
                input.OpeningOffice,
                input.CurrentOffice,
                input.Commercial,
                input.Status
            );

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            result.SetSuccess("Objetivo actualizado correctamente");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }
}
