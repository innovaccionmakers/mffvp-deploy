using Accounting.Integrations.AccountProcess;
using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Accounting.Presentation.GraphQL
{
    public interface IAccountingExperienceMutations
    {
        Task<GraphqlResult<AccountProcessResult>> AccountProcessAsync(
        AccountingInput input,
        IValidator<AccountingInput> validator,
        CancellationToken cancellationToken = default);       
    }
}
