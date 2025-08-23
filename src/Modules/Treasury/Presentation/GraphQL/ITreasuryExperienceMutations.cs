using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Treasury.Presentation.GraphQL.Input;

namespace Treasury.Presentation.GraphQL;

public interface ITreasuryExperienceMutations
{
    Task<GraphqlResult> AccountHandlerAsync(
        CreateAccountInput input,
        IValidator<CreateAccountInput> validator,
        CancellationToken cancellationToken = default
    );
    Task<GraphqlResult> TreasuryConfigHandlerAsync(
        TreasuryOperationInput input,
        IValidator<TreasuryOperationInput> validator,
        CancellationToken cancellationToken = default
    );
    Task<GraphqlResult> TreasuryOperationHandlerAsync(
        CreateTreasuryMovementInput input,
        IValidator<CreateTreasuryMovementInput> validator,
        CancellationToken cancellationToken = default
    );
}
