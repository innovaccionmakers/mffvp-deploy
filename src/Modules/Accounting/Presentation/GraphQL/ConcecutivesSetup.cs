using System.Linq;
using Accounting.Integrations.ConsecutivesSetup;
using Accounting.Presentation.DTOs;
using Accounting.Presentation.GraphQL.Inputs.ConsecutiveSetupInput;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MediatR;

namespace Accounting.Presentation.GraphQL;

public class ConcecutivesSetup(ISender mediator) : IConcecutivesSetup
{
    public async Task<GraphqlResult<ConsecutiveSetupPayloadDto>> HandleAsync(
        ConsecutiveSetupInput? input,
        CancellationToken cancellationToken = default)
    {
        var result = new GraphqlResult<ConsecutiveSetupPayloadDto>();

        try
        {
            if (input is null)
            {
                var queryResult = await mediator.Send(new GetConsecutivesSetupQuery(), cancellationToken);

                if (!queryResult.IsSuccess)
                {
                    result.AddError(queryResult.Error);
                    return result;
                }

                var consecutives = queryResult.Value
                    .Select(consecutive => new ConsecutiveSetupDto(
                        consecutive.Id,
                        consecutive.Nature,
                        consecutive.SourceDocument,
                        consecutive.Consecutive))
                    .ToList();

                result.SetSuccess(new ConsecutiveSetupPayloadDto(consecutives, null));
                return result;
            }

            var command = new UpdateConsecutiveSetupCommand(
                input.Id,
                input.Nature,
                input.SourceDocument,
                input.Consecutive);

            var updateResult = await mediator.Send(command, cancellationToken);

            if (!updateResult.IsSuccess)
            {
                result.AddError(updateResult.Error);
                return result;
            }

            var updatedConsecutive = updateResult.Value;

            result.SetSuccess(new ConsecutiveSetupPayloadDto(
                null,
                new ConsecutiveSetupDto(
                    updatedConsecutive.Id,
                    updatedConsecutive.Nature,
                    updatedConsecutive.SourceDocument,
                    updatedConsecutive.Consecutive)));

            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }
}
