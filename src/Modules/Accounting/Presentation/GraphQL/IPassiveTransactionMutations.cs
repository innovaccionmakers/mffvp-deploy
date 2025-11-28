using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Accounting.Presentation.GraphQL
{
    public interface IPassiveTransactionMutations
    {
        Task<GraphqlResult> CreatePassiveTransactionAsync(CreatePassiveTransactionInput input, IValidator<CreatePassiveTransactionInput> validator,CancellationToken cancellationToken = default);
        Task<GraphqlResult> UpdatePassiveTransactionAsync(UpdatePassiveTransactionInput input, IValidator<UpdatePassiveTransactionInput> validator,CancellationToken cancellationToken = default);
        Task<GraphqlResult> DeletePassiveTransactionAsync(DeletePassiveTransactionInput input, IValidator<DeletePassiveTransactionInput> validator, CancellationToken cancellationToken = default);
    }
}
