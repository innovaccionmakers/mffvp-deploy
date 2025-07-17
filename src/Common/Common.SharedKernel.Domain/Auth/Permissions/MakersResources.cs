namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersResources
{
    public const string users = nameof(users);

    public const string configurationParameters = nameof(configurationParameters);

    public const string activates = nameof(activates);
    public const string pensionRequirements = nameof(pensionRequirements);

    public const string auxiliaryInformations = nameof(auxiliaryInformations);
    public const string clientOperations = nameof(clientOperations);
    public const string contributiontx = nameof(contributiontx);


    //Operations
    public const string passiveIndividualOperations = nameof(passiveIndividualOperations);

    //Affiliates
    public const string affiliatesManagement = nameof(affiliatesManagement);
    public const string affiliatesObjective = nameof(affiliatesObjective);

    //Treasury
    public const string treasuryConcepts = nameof(treasuryConcepts);
    public const string treasuryLoadIndividualRecords = nameof(treasuryLoadIndividualRecords);

    //Accounting
    public const string accountingAccountSettings = nameof(accountingAccountSettings);
    public const string accountingGeneralSettings = nameof(accountingGeneralSettings);
    public const string accountingOperationSettings = nameof(accountingOperationSettings);
    public const string accountingTreasurySettings = nameof(accountingTreasurySettings);
    public const string accountingConceptSettings = nameof(accountingConceptSettings);
    public const string accountingGeneration = nameof(accountingGeneration);

    //Closing
    public const string closingLoadPnL = nameof(closingLoadPnL);
    public const string closingSimulation = nameof(closingSimulation);
    public const string closingExecution = nameof(closingExecution);

    //Reports
    public const string reportBalancesAndMovements = nameof(reportBalancesAndMovements);
    public const string reportTransfers = nameof(reportTransfers);
}
