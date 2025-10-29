namespace Accounting.Domain.Constants;


public static class AccountingTypes
{
    public const string Debit = "D";
    public const string Credit = "C";
}

public static class OperationTypeNames
{
    public const string Commission = "Comisión";
    public const string Yield = "Rendimientos";
    public const string Operation = "Operaciones";
    public const string Concepts = "Conceptos de Tesoreria";
    public const string AutomaticConcepts = "Conceptos Automáticos";
}

public static class ProcessTypes
{
    public const string AccountingFees = "AccountingFees";
    public const string AccountingReturns = "AccountingReturns";
    public const string AccountingOperations = "AccountingOperations";
    public const string AccountingConcepts = "AccountingConcepts";
    public const string AutomaticConcepts = "AutomaticConcepts";
    
    public static Dictionary<string, string> ProcessTypesDictionary = new()
    {
        { AccountingFees, "Comisiones" },
        { AccountingReturns, "Rendimientos" },
        { AccountingOperations, "Operaciones" },
        { AccountingConcepts, "Conceptos de Tesoreria" },
        { AutomaticConcepts, "Conceptos Automáticos" }
    };

    public static string GetTranslation(string processType) =>
        ProcessTypesDictionary.TryGetValue(processType, out var translation) ? translation : processType;
}

public static class AccountingActivity
{
    public const string Debit = "Débito";
    public const string Credit = "Crédito";
    public const string ContraDebitAccount = "Contra Débito";
    public const string ContraCreditAccount = "Contra Crédito";
}

