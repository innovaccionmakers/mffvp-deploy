using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL.Inputs;

namespace Operations.Presentation.GraphQL;

public interface IOperationsExperienceMutation
{
    public Task<GraphqlMutationResult<ContributionMutationResult>> RegisterContributionAsync(
        CreateContributionInput input,
        IValidator<CreateContributionInput> validator,
        CancellationToken cancellationToken = default

);
}
