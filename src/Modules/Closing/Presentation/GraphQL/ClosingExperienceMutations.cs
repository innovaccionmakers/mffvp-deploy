using Closing.Integrations.PreClosing.RunSimulation;
using Closing.Integrations.ProfitLosses.ProfitandLossLoad;
using Closing.Presentation.GraphQL.DTOs;
using Closing.Presentation.GraphQL.Inputs;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using FluentValidation;

using MediatR;

namespace Closing.Presentation.GraphQL;

public class ClosingExperienceMutations(IMediator mediator) : IClosingExperienceMutations
{
    public async Task<GraphqlResult<LoadProfitLossResult>> LoadProfitLossAsync(
        LoadProfitLossInput input,
        IValidator<LoadProfitLossInput> validator,
        CancellationToken cancellationToken = default
    )
    {
        var result = new GraphqlResult<LoadProfitLossResult>();
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

            result.SetSuccess(new LoadProfitLossResult(commandResult.Value), "Genial!, Se ha cargado el PyG del Portafolio");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }

    }

    public async Task<GraphqlResult<RunSimulationDto>> RunSimulationAsync(RunSimulationInput input, IValidator<RunSimulationInput> validator, CancellationToken cancellationToken = default)
    {
        var result = new GraphqlResult<RunSimulationDto>();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new RunSimulationCommand(
                input.PortfolioId,
                input.ClosingDate,
                input.IsClosing
            );

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var valueCommand = commandResult.Value;

            result.SetSuccess(new RunSimulationDto(
                valueCommand.Income,
                valueCommand.Expenses,
                valueCommand.Commissions,
                valueCommand.Costs,
                valueCommand.YieldToCredit,
                valueCommand.UnitValue,
                valueCommand.DailyProfitability,
                valueCommand.HasWarnings,
                valueCommand.Warnings?.Select(w => w.ToDto()).ToList() ?? new List<WarningItemDto>()
                ));

            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    public async Task<GraphqlResult<RunClosingDto>> RunClosingAsync(RunClosingInput input, IValidator<RunClosingInput> validator, CancellationToken cancellationToken = default)
    {
        var result = new GraphqlResult<RunClosingDto>();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new PrepareClosingCommand(
                input.PortfolioId,
                input.ClosingDate
            );

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var valueCommand = commandResult.Value;

            var warningsDto = valueCommand.Warnings?.Select(w => w.ToDto()).ToList()
                  ?? new List<WarningItemDto>();

            result.SetSuccess(new RunClosingDto(
                valueCommand.Income,
                valueCommand.Expenses,
                valueCommand.Commissions,
                valueCommand.Costs,
                valueCommand.YieldToCredit,
                valueCommand.UnitValue,
                valueCommand.DailyProfitability,
                valueCommand.HasWarnings,
                warningsDto
                ));

            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    public async Task<GraphqlResult<ConfirmClosingDto>> ConfirmClosingAsync(ConfirmClosingInput input, IValidator<ConfirmClosingInput> validator, CancellationToken cancellationToken = default)
    {
        var result = new GraphqlResult<ConfirmClosingDto>();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new ConfirmClosingCommand(
                input.PortfolioId,
                input.ClosingDate
            );

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var valueCommand = commandResult.Value;

            var warningsDto = valueCommand.Warnings?.Select(w => w.ToDto()).ToList()
                  ?? new List<WarningItemDto>();


            result.SetSuccess(new ConfirmClosingDto(
                valueCommand.PortfolioId,
                valueCommand.ClosingDate,
                valueCommand.HasWarnings,
                warningsDto
                ));

            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    public async Task<GraphqlResult<CancelClosingDto>> CancelClosingAsync(CancelClosingInput input, IValidator<CancelClosingInput> validator, CancellationToken cancellationToken = default)
    {
        var result = new GraphqlResult<CancelClosingDto>();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new CancelClosingCommand(
                input.PortfolioId,
                input.ClosingDate
            );

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var valueCommand = commandResult.Value;

            result.SetSuccess(new CancelClosingDto(
                valueCommand.PortfolioId, 
                valueCommand.ClosingDate, 
                valueCommand.IsCanceled
                ));

            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }
}