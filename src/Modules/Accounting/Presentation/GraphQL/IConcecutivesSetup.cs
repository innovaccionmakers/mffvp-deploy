using Accounting.Presentation.DTOs;
using Accounting.Presentation.GraphQL.Inputs.ConsecutiveSetupInput;
using Common.SharedKernel.Presentation.Results;

namespace Accounting.Presentation.GraphQL;

public interface IConcecutivesSetup
{
    Task<GraphqlResult<ConsecutiveSetupPayloadDto>> HandleAsync(
        ConsecutiveSetupInput? input,
        CancellationToken cancellationToken = default);
}
