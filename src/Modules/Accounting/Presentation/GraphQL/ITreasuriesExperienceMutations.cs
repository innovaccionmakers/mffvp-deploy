using Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput;
using Accounting.Presentation.GraphQL.Inputs.TreasuriesInput;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Accounting.Presentation.GraphQL
{
    public interface ITreasuriesExperienceMutations
    {
        Task<GraphqlResult> CreateTreasuryAsync(CreateTreasuryInput input, IValidator<CreateTreasuryInput> validator,CancellationToken cancellationToken = default);
        Task<GraphqlResult> UpdateTreasuryAsync(UpdateTreasuryInput input, IValidator<UpdateTreasuryInput> validator,CancellationToken cancellationToken = default);
        Task<GraphqlResult> DeleteTreasuryAsync(DeleteTreasuryInput input, IValidator<DeleteTreasuryInput> validator, CancellationToken cancellationToken = default);
    }
}
