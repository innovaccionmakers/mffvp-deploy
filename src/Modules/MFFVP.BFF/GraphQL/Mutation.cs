using Accounting.Integrations.AccountProcess;
using Accounting.Presentation.GraphQL;
using Accounting.Presentation.GraphQL.Inputs;
using Associate.Presentation.GraphQL;
using Associate.Presentation.GraphQL.Inputs;
using Closing.Presentation.GraphQL;
using Closing.Presentation.GraphQL.DTOs;
using Closing.Presentation.GraphQL.Inputs;

using Common.SharedKernel.Domain.Auth.Permissions;
using Common.SharedKernel.Presentation.Results;

using FluentValidation;

using HotChocolate.Authorization;

using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Operations.Presentation.GraphQL.Inputs;

using Products.Presentation.DTOs;
using Products.Presentation.GraphQL;
using Products.Presentation.GraphQL.Input;

using Treasury.Presentation.GraphQL;
using Treasury.Presentation.GraphQL.Input;

namespace MFFVP.BFF.GraphQL;

[Authorize]
public class Mutation
{
    [GraphQLName("crearAporte")]
    [Authorize(Policy = MakersPermissionsOperations.PolicyExecuteIndividualOperations)]
    public async Task<GraphqlResult<ContributionMutationResult>> RegisterContribution([GraphQLName("aporte")] CreateContributionInput contribution,
                                                                        IValidator<CreateContributionInput> validator,
                                                                       [Service] IOperationsExperienceMutation operationsMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await operationsMutations.RegisterContributionAsync(contribution, validator, cancellationToken);
    }

    [GraphQLName("crearNotaDebito")]
    [Authorize(Policy = MakersPermissionsOperations.PolicyCreateDebitNote)]
    public async Task<GraphqlResult<DebitNoteMutationResult>> RegisterDebitNote([GraphQLName("notaDebito")] CreateDebitNoteInput debitNote,
                                                                        IValidator<CreateDebitNoteInput> validator,
                                                                       [Service] IOperationsExperienceMutation operationsMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await operationsMutations.RegisterDebitNoteAsync(debitNote, validator, cancellationToken);
    }

    [GraphQLName("registrarAnulaciones")]
    [Authorize(Policy = MakersPermissionsOperations.PolicyCancelIndividualOperations)]
    public async Task<GraphqlResult<VoidedTransactionsMutationResult>> RegisterVoids([GraphQLName("anulacion")] CreateVoidsInput input,
                                                                        IValidator<CreateVoidsInput> validator,
                                                                       [Service] IOperationsExperienceMutation operationsMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await operationsMutations.RegisterVoidsAsync(input, validator, cancellationToken);
    }

    //Associate mutations
    [GraphQLName("crearActivacion")]
    [Authorize(Policy = MakersPermissionsAffiliates.PolicyActivateAffiliateManagement)]
    public async Task<GraphqlResult> RegisterActivation([GraphQLName("activacion")] CreateActivateInput activation,
                                                                        IValidator<CreateActivateInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.RegisterActivateAsync(activation, validator, cancellationToken);
    }

    [GraphQLName("actualizarActivacion")]
    [Authorize(Policy = MakersPermissionsAffiliates.PolicyUpdateAffiliateManagement)]
    public async Task<GraphqlResult> UpdateActivation([GraphQLName("activacion")] UpdateActivateInput activation,
                                                                        IValidator<UpdateActivateInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.UpdateActivateAsync(activation, validator, cancellationToken);
    }

    [GraphQLName("crearRequisitosPension")]
    public async Task<GraphqlResult> RegisterPensionRequirements([GraphQLName("requisitoPension")] CreatePensionRequirementInput pensionRequirement,
                                                                        IValidator<CreatePensionRequirementInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.RegisterPensionRequirementsAsync(pensionRequirement, validator, cancellationToken);
    }

    [GraphQLName("actualizarRequisitosPension")]
    public async Task<GraphqlResult> UpdatePensionRequirements([GraphQLName("requisitoPension")] UpdatePensionRequirementInput pensionRequirement,
                                                                        IValidator<UpdatePensionRequirementInput> validator,
                                                                       [Service] IAssociatesExperienceMutations associatesMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await associatesMutations.UpdatePensionRequirementsAsync(pensionRequirement, validator, cancellationToken);
    }

    //product mutations
    [GraphQLName("crearObjetivo")]
    [Authorize(Policy = MakersPermissionsAffiliates.PolicyCreateGoal)]
    public async Task<GraphqlResult<GoalMutationResult>> RegisterGoal([GraphQLName("objetivo")] CreateGoalInput goal,
                                                        IValidator<CreateGoalInput> validator,
                                                        [Service] IProductsExperienceMutations productsMutations,
                                                        CancellationToken cancellationToken)
    {
        return await productsMutations.RegisterGoalAsync(goal, validator, cancellationToken);
    }

    [GraphQLName("actualizarObjetivo")]
    [Authorize(Policy = MakersPermissionsAffiliates.PolicyUpdateGoal)]
    public async Task<GraphqlResult> UpdateGoal([GraphQLName("objetivo")] UpdateGoalInput goal,
                                                        IValidator<UpdateGoalInput> validator,
                                                        [Service] IProductsExperienceMutations productsMutations,
                                                        CancellationToken cancellationToken)
    {
        return await productsMutations.UpdateGoalAsync(goal, validator, cancellationToken);
    }

    [GraphQLName("fichaTecnica")]
    public async Task<GraphqlResult> TechnicalSheet([GraphQLName("fechaCierre")] DateOnly closingDate,
                                                        [Service] IProductsExperienceMutations productsMutations,
                                                        CancellationToken cancellationToken)
    {
        return await productsMutations.SaveTechnicalSheetAsync(closingDate, cancellationToken);
    }

    //closing mutations
    [GraphQLName("cargarPerdidasGanancias")]
    [Authorize(Policy = MakersPermissionsClosing.PolicyCreateLoadProfitAndLost)]
    public async Task<GraphqlResult<LoadProfitLossResult>> LoadProfitLoss([GraphQLName("perdidaganancia")] LoadProfitLossInput input,
                                                        IValidator<LoadProfitLossInput> validator,
                                                        [Service] IClosingExperienceMutations closingMutations,
                                                        CancellationToken cancellationToken)
    {
        return await closingMutations.LoadProfitLossAsync(input, validator, cancellationToken);
    }

    //Treasury mutations
    [GraphQLName("crearCuentaBancaria")]
    public async Task<GraphqlResult> AccountHandler([GraphQLName("cuentaBancaria")] CreateAccountInput bankAccount,
                                                                        IValidator<CreateAccountInput> validator,
                                                                       [Service] ITreasuryExperienceMutations treasuryMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await treasuryMutations.AccountHandlerAsync(bankAccount, validator, cancellationToken);
    }

    [GraphQLName("procesarConceptoTesoreria")]
    public async Task<GraphqlResult> TreasuryConfigHandler([GraphQLName("conceptoTesoreria")] TreasuryOperationInput treasuryMovement,
                                                                        IValidator<TreasuryOperationInput> validator,
                                                                       [Service] ITreasuryExperienceMutations treasuryMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await treasuryMutations.TreasuryConfigHandlerAsync(treasuryMovement, validator, cancellationToken);
    }

    [GraphQLName("crearMovimientoTesoreria")]
    public async Task<GraphqlResult> TreasuryOperationHandler([GraphQLName("movimientoTesoreria")] CreateTreasuryMovementInput treasuryMovement,
                                                                        IValidator<CreateTreasuryMovementInput> validator,
                                                                       [Service] ITreasuryExperienceMutations treasuryMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await treasuryMutations.TreasuryOperationHandlerAsync(treasuryMovement, validator, cancellationToken);
    }

    [GraphQLName("simulacionEjecucion")]
    [Authorize(Policy = MakersPermissionsClosing.PolicyExecuteSimulation)]
    public async Task<GraphqlResult<RunSimulationDto>> RunSimulationAsync([GraphQLName("simulacion")] RunSimulationInput input,
                                                        IValidator<RunSimulationInput> validator,
                                                        [Service] IClosingExperienceMutations closingMutations,
                                                        CancellationToken cancellationToken)
    {
        return await closingMutations.RunSimulationAsync(input, validator, cancellationToken);
    }

    [GraphQLName("cierreEjecucion")]
    [Authorize(Policy = MakersPermissionsClosing.PolicyExecuteClosure)]
    public async Task<GraphqlResult<RunClosingDto>> RunClosingAsync([GraphQLName("cierre")] RunClosingInput input,
                                                       IValidator<RunClosingInput> validator,
                                                       [Service] IClosingExperienceMutations closingMutations,
                                                       CancellationToken cancellationToken)
    {
        return await closingMutations.RunClosingAsync(input, validator, cancellationToken);
    }


    [GraphQLName("cierreConfirmacion")]
    public async Task<GraphqlResult<ConfirmClosingDto>> ConfirmClosingAsync([GraphQLName("cierre")] ConfirmClosingInput input,
                                                       IValidator<ConfirmClosingInput> validator,
                                                       [Service] IClosingExperienceMutations closingMutations,
                                                       CancellationToken cancellationToken)
    {
        return await closingMutations.ConfirmClosingAsync(input, validator, cancellationToken);
    }

    [GraphQLName("cierreCancelacion")]
    public async Task<GraphqlResult<CancelClosingDto>> CancelClosingAsync([GraphQLName("cierre")] CancelClosingInput input,
                                                       IValidator<CancelClosingInput> validator,
                                                       [Service] IClosingExperienceMutations closingMutations,
                                                       CancellationToken cancellationToken)
    {
        return await closingMutations.CancelClosingAsync(input, validator, cancellationToken);
    }

    [GraphQLName("procesoCuenta")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyGenerateGeneration)]
    public async Task<GraphqlResult<AccountProcessResult>> AccountProcess([GraphQLName("cuenta")] AccountingInput input,
                                                       IValidator<AccountingInput> validator,
                                                       [Service] IAccountProcessExperienceMutations accountProcessMutations,
                                                       CancellationToken cancellationToken)
    {
        return await accountProcessMutations.AccountProcessAsync(input, validator, cancellationToken);
    }   
}
