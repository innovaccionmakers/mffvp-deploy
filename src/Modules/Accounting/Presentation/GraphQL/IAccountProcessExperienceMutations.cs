using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Accounting.Presentation.GraphQL
{
    public interface IAccountProcessExperienceMutations
    {
        Task<GraphqlResult<string>> AccountProcessAsync(
        AccountingInput input,
        IValidator<AccountingInput> validator,
        CancellationToken cancellationToken = default);
    }
}
