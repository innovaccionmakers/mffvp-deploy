using Accounting.Integrations.AccountProcess;
using Accounting.Presentation.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.DeleteConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.UpdateConfigurationGeneral;
using Accounting.Presentation.DTOs;
using Accounting.Presentation.GraphQL;
using Accounting.Presentation.GraphQL.Inputs;
using Accounting.Presentation.GraphQL.Inputs.AccountingInput;
using Accounting.Presentation.GraphQL.Inputs.ConsecutiveSetupInput;
using Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput;
using Accounting.Presentation.GraphQL.Inputs.TreasuriesInput;
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
using Reports.Presentation.GraphQL;
using Reports.Presentation.GraphQL.Dtos;
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
    [Authorize(Policy = MakersPermissionsOperations.PolicyCreateAccountingNote)]
    public async Task<GraphqlResult<DebitNoteMutationResult>> RegisterDebitNote([GraphQLName("notaDebito")] CreateDebitNoteInput debitNote,
                                                                        IValidator<CreateDebitNoteInput> validator,
                                                                       [Service] IOperationsExperienceMutation operationsMutations,
                                                                       CancellationToken cancellationToken)
    {
        return await operationsMutations.RegisterDebitNoteAsync(debitNote, validator, cancellationToken);
    }

    [GraphQLName("registrarAnulaciones")]
    [Authorize(Policy = MakersPermissionsOperations.PolicyCancelOperations)]
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
    //[Authorize(Policy = MakersPermissionsAccounting.PolicyGenerateGeneration)]
    public async Task<GraphqlResult<AccountProcessResult>> AccountProcessAsync([GraphQLName("cuenta")] AccountingInput input,
                                                       IValidator<AccountingInput> validator,
                                                       [Service] IAccountingExperienceMutations accountProcessMutations,
                                                       CancellationToken cancellationToken)
    {
        return await accountProcessMutations.AccountProcessAsync(input, validator, cancellationToken);
    }

    [GraphQLName("crearTransaccionPasiva")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyCreatePassiveTransaction)]
    public async Task<GraphqlResult> CreatePassiveTransactionAsync([GraphQLName("transaccionPasiva")] CreatePassiveTransactionInput input,
                                                   IValidator<CreatePassiveTransactionInput> validator,
                                                   [Service] IPassiveTransactionExperienceMutations passiveTransactionMutations,
                                                   CancellationToken cancellationToken)
    {
        return await passiveTransactionMutations.CreatePassiveTransactionAsync(input, validator, cancellationToken);
    }

    [GraphQLName("actualizarTransaccionPasiva")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyUpdatePassiveTransaction)]
    public async Task<GraphqlResult> UpdatePassiveTransactionAsync([GraphQLName("transaccionPasiva")] UpdatePassiveTransactionInput input,
                                                   IValidator<UpdatePassiveTransactionInput> validator,
                                                   [Service] IPassiveTransactionExperienceMutations passiveTransactionMutations,
                                                   CancellationToken cancellationToken)
    {
        return await passiveTransactionMutations.UpdatePassiveTransactionAsync(input, validator, cancellationToken);
    }

    [GraphQLName("eliminarTransaccionPasiva")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyDeletePassiveTransaction)]
    public async Task<GraphqlResult> DeletePassiveTransactionAsync([GraphQLName("transaccionPasiva")] DeletePassiveTransactionInput input,
                                                   IValidator<DeletePassiveTransactionInput> validator,
                                                   [Service] IPassiveTransactionExperienceMutations passiveTransactionMutations,
                                                   CancellationToken cancellationToken)
    {
        return await passiveTransactionMutations.DeletePassiveTransactionAsync(input, validator, cancellationToken);
    }

    [GraphQLName("consecutivosContables")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyCreateConsecutive)]
    public async Task<GraphqlResult<ConsecutiveSetupPayloadDto>> HandleConsecutivesSetup(
        [GraphQLName("consecutivo")] ConsecutiveSetupInput? input,
        [Service] IConcecutivesSetup concecutivesSetup,
        CancellationToken cancellationToken)
    {
        return await concecutivesSetup.HandleAsync(input, cancellationToken);
    }

    [GraphQLName("crearTesoreria")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyCreateTreasury)]
    public async Task<GraphqlResult> CreateTreasuryAsync([GraphQLName("tesoreria")] CreateTreasuryInput input,
                                                   IValidator<CreateTreasuryInput> validator,
                                                   [Service] ITreasuriesExperienceMutations treasuriesMutations,
                                                   CancellationToken cancellationToken)
    {
        return await treasuriesMutations.CreateTreasuryAsync(input, validator, cancellationToken);
    }

    [GraphQLName("actualizarTesoreria")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyUpdateTreasury)]
    public async Task<GraphqlResult> UpdateTreasuryAsync([GraphQLName("tesoreria")] UpdateTreasuryInput input,
                                                   IValidator<UpdateTreasuryInput> validator,
                                                   [Service] ITreasuriesExperienceMutations treasuriesMutations,
                                                   CancellationToken cancellationToken)
    {
        return await treasuriesMutations.UpdateTreasuryAsync(input, validator, cancellationToken);
    }

    [GraphQLName("eliminarTesoreria")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyDeleteTreasury)]
    public async Task<GraphqlResult> DeleteTreasuryAsync([GraphQLName("tesoreria")] DeleteTreasuryInput input,
                                                   IValidator<DeleteTreasuryInput> validator,
                                                   [Service] ITreasuriesExperienceMutations treasuriesMutations,
                                                   CancellationToken cancellationToken)
    {
        return await treasuriesMutations.DeleteTreasuryAsync(input, validator, cancellationToken);
    }

    [GraphQLName("crearConcepto")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyCreateConcept)]
    public async Task<GraphqlResult> CreateConceptAsync([GraphQLName("concepto")] CreateConceptInput input,
                                                   IValidator<CreateConceptInput> validator,
                                                   [Service] IConceptMutations conceptMutations,
                                                   CancellationToken cancellationToken)
    {
        return await conceptMutations.CreateConceptAsync(input, validator, cancellationToken);
    }

    [GraphQLName("actualizarConcepto")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyUpdateConcept)]
    public async Task<GraphqlResult> UpdateConceptAsync([GraphQLName("concepto")] UpdateConceptInput input,
                                                   IValidator<UpdateConceptInput> validator,
                                                   [Service] IConceptMutations conceptMutations,
                                                   CancellationToken cancellationToken)
    {
        return await conceptMutations.UpdateConceptAsync(input, validator, cancellationToken);
    }

    [GraphQLName("eliminarConcepto")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyDeleteConcept)]
    public async Task<GraphqlResult> DeleteConceptAsync([GraphQLName("concepto")] DeleteConceptInput input,
                                                   IValidator<DeleteConceptInput> validator,
                                                   [Service] IConceptMutations conceptMutations,
                                                   CancellationToken cancellationToken)
    {
        return await conceptMutations.DeleteConceptAsync(input, validator, cancellationToken);
    }

    [GraphQLName("crearConfiguracionGeneral")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyCreateGeneralConfiguration)]
    public async Task<GraphqlResult> CreateConfiguracionGeneralAsync([GraphQLName("configuracionGeneral")] CreateConfigurationGeneralInput input,
                                                   IValidator<CreateConfigurationGeneralInput> validator,
                                                   [Service] IConfigurationGeneralsExperienceMutations configurationGeneralsMutations,
                                                   CancellationToken cancellationToken)
    {
        return await configurationGeneralsMutations.CreateConfiguracionGeneralAsync(input, validator, cancellationToken);
    }

    [GraphQLName("actualizarConfiguracionGeneral")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyUpdateGeneralConfiguration)]
    public async Task<GraphqlResult> UpdateConfiguracionGeneralAsync([GraphQLName("configuracionGeneral")] UpdateConfigurationGeneralInput input,
                                                   IValidator<UpdateConfigurationGeneralInput> validator,
                                                   [Service] IConfigurationGeneralsExperienceMutations configurationGeneralsMutations,
                                                   CancellationToken cancellationToken)
    {
        return await configurationGeneralsMutations.UpdateConfiguracionGeneralAsync(input, validator, cancellationToken);
    }

    [GraphQLName("eliminarConfiguracionGeneral")]
    [Authorize(Policy = MakersPermissionsAccounting.PolicyDeleteGeneralConfiguration)]
    public async Task<GraphqlResult> DeleteConfiguracionGeneralAsync([GraphQLName("configuracionGeneral")] DeleteConfigurationGeneralInput input,
                                                   IValidator<DeleteConfigurationGeneralInput> validator,
                                                   [Service] IConfigurationGeneralsExperienceMutations configurationGeneralsMutations,
                                                   CancellationToken cancellationToken)
    {
        return await configurationGeneralsMutations.DeleteConfiguracionGeneralAsync(input, validator, cancellationToken);
    }

    [GraphQLName("procesarCargaDiariaReportes")]
    public async Task<GraphqlResult<ProcessDailyDataDto>> ProcessDailyReportsAsync(
    [GraphQLName("carga")] ProcessDailyDataInput input,
    IValidator<ProcessDailyDataInput> validator,
    [Service] IReportsExperienceMutations reportsMutations,
    CancellationToken cancellationToken)
    {
        return await reportsMutations.ProcessDailyDataAsync(input, validator, cancellationToken);
    }
}
