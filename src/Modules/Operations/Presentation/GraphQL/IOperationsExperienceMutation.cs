using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL.Inputs;

namespace Operations.Presentation.GraphQL;

public interface IOperationsExperienceMutation
{
    public Task<ContributionMutationResult> RegisterContributionAsync(
        CreateContributionInput input,
        CancellationToken cancellationToken = default
        );
}
