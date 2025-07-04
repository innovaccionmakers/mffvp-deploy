using FluentValidation;

using Operations.Integrations.Contributions.CreateContribution;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Operations.Presentation.GraphQL.Inputs;

namespace MFFVP.BFF.GraphQL;

public class Mutation
{
    [GraphQLName("crearAporte")]
    public async Task<ContributionMutationResult> RegisterContribution([GraphQLName("aporte")] CreateContributionInput contribution,
                                                                        IValidator<CreateContributionInput> validator,
                                                                       [Service] IOperationsExperienceMutation operationsMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await operationsMutations.RegisterContributionAsync(contribution, validator, cancellationToken);
    }
}
