
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Reports.Integrations.LoadingInfo.Commands;
using Reports.Presentation.GraphQL.Dtos;
using MediatR;

namespace Reports.Presentation.GraphQL;

public sealed class ReportsExperienceMutations(IMediator mediator) : IReportsExperienceMutations
{
    public async Task<GraphqlResult<ProcessDailyDataDto>> ProcessDailyDataAsync(
        ProcessDailyDataInput input,
        IValidator<ProcessDailyDataInput> validator,
        CancellationToken cancellationToken = default)
    {
        var result = new GraphqlResult<ProcessDailyDataDto>();

        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);
            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new ProcessDailyDataCommand(
                portfolioId: input.PortfolioId,
                closingDateUtc: input.ClosingDateUtc,
                etlSelection: input.EtlSelection);

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var value = commandResult.Value;

            var dto = new ProcessDailyDataDto(
                ExecutionId: value.ExecutionId,
                PortfolioId: value.PortfolioId,
                ClosingDate: value.ClosingDate,
                Status: value.Status,
                Selection: value.Selection,
                Steps: value.Steps);

            result.SetSuccess(dto, "ETLs ejecutados.");
            return result;
        }
        catch (Exception exception)
        {
            result.AddError(new Error("EXCEPTION", exception.Message, ErrorType.Failure));
            return result;
        }
    }
}
