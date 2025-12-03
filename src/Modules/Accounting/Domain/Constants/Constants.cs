using Accounting.Domain.Concepts;
using DocumentFormat.OpenXml.Drawing.Diagrams;

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
    public const string Operation = "Aporte";
    public const string DebitNote = "Nota Débito";
    public const string Concepts = "Conceptos de Tesoreria";
    public const string AutomaticConcepts = "Conceptos Automáticos";
    public const string AdjustYields = "Ajuste Rendimientos";
    public const string AutomaticConcept = "Concepto Automático";
    public const string AutomaticConceptAccountingNote = "Concepto Automatico Nota Contable";
}

public static class SourceTypes
{
    public const string ExtraYield = "Rendimiento Extra";
    public const string AutomaticConcept = "Concepto Automatico";
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
        { AccountingOperations, "Aportes" },
        { AccountingConcepts, "Conceptos de Tesoreria" },
        { AutomaticConcepts, "Conceptos Automáticos" }
    };

    public static string GetTranslation(string processType) =>
        ProcessTypesDictionary.TryGetValue(processType, out var translation) ? translation : processType;

    public static readonly string[] Process =
    {
        //AccountingFees,
        //AccountingReturns,
        //AccountingOperations,
        //AccountingConcepts,
        AutomaticConcepts
    };
}

public static class AccountingActivity
{
    public const string Debit = "Débito";
    public const string Credit = "Crédito";
    public const string ContraDebitAccount = "Contra Débito";
    public const string ContraCreditAccount = "Contra Crédito";
}

public static class NatureTypes
{
    public const string Income = "I";
    public const string Egress = "E";
}


public static class AccountingReportConstants
{
    public const string FORINT = "FA";
    public const string NBAINT = "0000000000";
    public const string VBAINT = "0";
    public const string NVBINT = "0";
    public const string FEMINT = "0";
    public const string FVEINT = "0";
    public const string POINT = ".";
    public const string NDOINT = "00";
    public const char ESTINT = ' ';
    public const string IncomeCode = "CO";
    public const string EgressCode = "CH";
    public const string ZeroValue = "000000000000000.00";
    public const char BlankSpace = ' ';
    public const int MaxConsecutiveNumber = 9999999;
}

public static class ConceptsTypeNames
{ 
    public const string PerformanceAdjustmentAccountingNote = "Ajuste Rendimiento Nota Contable";
    public const string AdjustYieldsIncome = "Ajuste Rendimiento NC Ingreso";
}
