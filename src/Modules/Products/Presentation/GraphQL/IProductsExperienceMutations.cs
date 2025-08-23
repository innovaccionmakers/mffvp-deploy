using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Products.Presentation.DTOs;
using Products.Presentation.GraphQL.Input;

namespace Products.Presentation.GraphQL;

public interface IProductsExperienceMutations
{
    public Task<GraphqlResult<GoalMutationResult>> RegisterGoalAsync(
        CreateGoalInput input,
        IValidator<CreateGoalInput> validator,
        CancellationToken cancellationToken = default
    );

    Task<GraphqlResult> UpdateGoalAsync(
        UpdateGoalInput input,
        IValidator<UpdateGoalInput> validator,
        CancellationToken cancellationToken = default
    );

    Task<GraphqlResult> SaveTechnicalSheetAsync(DateOnly closingDate,
                                                        CancellationToken cancellationToken = default);
}
