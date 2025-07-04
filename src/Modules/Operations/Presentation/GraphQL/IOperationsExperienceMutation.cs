using FluentValidation;

using Operations.Integrations.Contributions.CreateContribution;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL.Inputs;

namespace Operations.Presentation.GraphQL;

public interface IOperationsExperienceMutation
{
    public Task<ContributionMutationResult> RegisterContributionAsync(
        CreateContributionInput input,
        IValidator<CreateContributionInput> validator,
        CancellationToken cancellationToken = default
        );
}
