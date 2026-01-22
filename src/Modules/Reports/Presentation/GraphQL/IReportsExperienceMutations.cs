

using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Reports.Presentation.GraphQL.Dtos;

namespace Reports.Presentation.GraphQL;

public interface IReportsExperienceMutations
{
    Task<GraphqlResult<ProcessDailyDataDto>> ProcessDailyDataAsync(
        ProcessDailyDataInput input,
        IValidator<ProcessDailyDataInput> validator,
        CancellationToken cancellationToken = default);
}