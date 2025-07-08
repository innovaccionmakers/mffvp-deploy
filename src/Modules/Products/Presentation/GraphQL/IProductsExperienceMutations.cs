using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Products.Presentation.DTOs;
using Products.Presentation.GraphQL.Input;

namespace Products.Presentation.GraphQL;

public interface IProductsExperienceMutations
{
    public Task<GraphqlMutationResult<GoalMutationResult>> RegisterGoalAsync(
        CreateGoalInput input,
        IValidator<CreateGoalInput> validator,
        CancellationToken cancellationToken = default
    );
}
