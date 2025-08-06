using Associate.Presentation.GraphQL;
using Associate.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Operations.Presentation.GraphQL.Inputs;
using Products.Presentation.DTOs;
using Products.Presentation.GraphQL;
using Products.Presentation.GraphQL.Input;
using Treasury.Presentation.GraphQL;
using Treasury.Presentation.GraphQL.Input;
using Closing.Presentation.GraphQL;
using Closing.Presentation.GraphQL.Inputs;
using Closing.Presentation.GraphQL.DTOs;
using HotChocolate.Authorization;

namespace MFFVP.BFF.GraphQL;

[Authorize]
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

    [GraphQLName("actualizarActivacion")]
    public async Task<GraphqlMutationResult> UpdateActivation([GraphQLName("activacion")] UpdateActivateInput activation,
                                                                        IValidator<UpdateActivateInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.UpdateActivateAsync(activation, validator, cancellationToken);
    }

    [GraphQLName("crearRequisitosPension")]
    public async Task<GraphqlMutationResult> RegisterPensionRequirements([GraphQLName("requisitoPension")] CreatePensionRequirementInput pensionRequirement,
                                                                        IValidator<CreatePensionRequirementInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.RegisterPensionRequirementsAsync(pensionRequirement, validator, cancellationToken);
    }

    [GraphQLName("actualizarRequisitosPension")]
    public async Task<GraphqlMutationResult> UpdatePensionRequirements([GraphQLName("requisitoPension")] UpdatePensionRequirementInput pensionRequirement,
                                                                        IValidator<UpdatePensionRequirementInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.UpdatePensionRequirementsAsync(pensionRequirement, validator, cancellationToken);
    }

    //product mutations
    [GraphQLName("crearObjetivo")]
    public async Task<GraphqlMutationResult<GoalMutationResult>> RegisterGoal([GraphQLName("objetivo")] CreateGoalInput goal,
                                                        IValidator<CreateGoalInput> validator,
                                                        [Service] IProductsExperienceMutations productsMutations,
                                                        CancellationToken cancellationToken)
    {
        return await productsMutations.RegisterGoalAsync(goal, validator, cancellationToken);
    }

    [GraphQLName("actualizarObjetivo")]
    public async Task<GraphqlMutationResult> UpdateGoal([GraphQLName("objetivo")] UpdateGoalInput goal,
                                                        IValidator<UpdateGoalInput> validator,
                                                        [Service] IProductsExperienceMutations productsMutations,
                                                        CancellationToken cancellationToken)
    {
        return await productsMutations.UpdateGoalAsync(goal, validator, cancellationToken);
    }

    //closing mutations
    [GraphQLName("cargarPerdidasGanancias")]
    public async Task<GraphqlMutationResult<LoadProfitLossResult>> LoadProfitLoss([GraphQLName("perdidaganancia")] LoadProfitLossInput input,
                                                        IValidator<LoadProfitLossInput> validator,
                                                        [Service] IClosingExperienceMutations closingMutations,
                                                        CancellationToken cancellationToken)
    {
        return await closingMutations.LoadProfitLossAsync(input, validator, cancellationToken);
    }

    //Treasury mutations
    [GraphQLName("crearCuentaBancaria")]
    public async Task<GraphqlMutationResult> AccountHandler([GraphQLName("cuentaBancaria")] CreateAccountInput bankAccount,
                                                                        IValidator<CreateAccountInput> validator,
                                                                       [Service] ITreasuryExperienceMutations treasuryMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await treasuryMutations.AccountHandlerAsync(bankAccount, validator, cancellationToken);
    }

    [GraphQLName("crearConceptosTesoreria")]
    public async Task<GraphqlMutationResult> TreasuryConfigHandler([GraphQLName("conceptoTesoreria")] CreateTreasuryOperationInput treasuryMovement,
                                                                        IValidator<CreateTreasuryOperationInput> validator,
                                                                       [Service] ITreasuryExperienceMutations treasuryMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await treasuryMutations.TreasuryConfigHandlerAsync(treasuryMovement, validator, cancellationToken);
    }

    [GraphQLName("crearMovimientoTesoreria")]
    public async Task<GraphqlMutationResult> TreasuryOperationHandler([GraphQLName("movimientoTesoreria")] CreateTreasuryMovementInput treasuryMovement,
                                                                        IValidator<CreateTreasuryMovementInput> validator,
                                                                       [Service] ITreasuryExperienceMutations treasuryMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await treasuryMutations.TreasuryOperationHandlerAsync(treasuryMovement, validator, cancellationToken);
    }

    [GraphQLName("simulacionEjecucion")]
    public async Task<GraphqlMutationResult> RunSimulationAsync([GraphQLName("simulacion")] RunSimulationInput input,
                                                        IValidator<RunSimulationInput> validator,
                                                        [Service] IClosingExperienceMutations closingMutations,
                                                        CancellationToken cancellationToken)
    {
        return await closingMutations.RunSimulationAsync(input, validator, cancellationToken);
    }
}
