using Associate.Presentation.GraphQL;
using Associate.Presentation.GraphQL.Inputs;
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

    //Associate mutations
    [GraphQLName("crearActivacion")]
    public async Task<GraphqlMutationResult> RegisterActivation([GraphQLName("activacion")] CreateActivateInput activation,
                                                                        IValidator<CreateActivateInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.RegisterActivateAsync(activation, validator, cancellationToken);
    }

    [GraphQLName("actualizarRequisitosPension")]
    public async Task<GraphqlMutationResult> UpdatePensionRequirements([GraphQLName("requisitoPension")] UpdatePensionRequirementInput pensionRequirement,
                                                                        IValidator<UpdatePensionRequirementInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.UpdatePensionRequirementsAsync(pensionRequirement, validator, cancellationToken);
    }
}
