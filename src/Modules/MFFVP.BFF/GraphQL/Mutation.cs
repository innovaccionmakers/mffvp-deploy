using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Operations.Presentation.GraphQL.Inputs;

namespace MFFVP.BFF.GraphQL;

public class Mutation
{
    [GraphQLName("crearAporte")]
    public async Task<GraphqlMutationResult<ContributionMutationResult>> RegisterContribution([GraphQLName("aporte")] CreateContributionInput contribution,
                                                                        IValidator<CreateContributionInput> validator,
                                                                       [Service] IOperationsExperienceMutation operationsMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await operationsMutations.RegisterContributionAsync(contribution, validator, cancellationToken);
    }
}
