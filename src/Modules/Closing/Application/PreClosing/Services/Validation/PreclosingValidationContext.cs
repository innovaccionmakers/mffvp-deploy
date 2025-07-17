namespace Closing.Application.PreClosing.Services.Validation;

public sealed class PreclosingValidationContext
{
    // Requeridos
    public DateOnly ClosingDate { get; internal set; }
    public DateOnly CurrentDate { get; internal set; }
    public bool IsFirstClosingDay { get; internal set; }

    // Estado de datos primer día
    public bool ProfitAndLossExists { get; internal set; } = false;
    public bool TreasuryMovementsExists { get; internal set; } = false;
    public bool HasClientOperations { get; internal set; } = false;

    // Comisión administrativa
    public int AdminCommissionCount { get; internal set; } = 0;
    public bool AdminCommissionIsNumber { get; internal set; } = false;
    public bool AdminCommissionBetween0And100 { get; internal set; } = false;

    // ¿Ya existe cierre generado?
    public bool ExistsClosingGenerated { get; internal set; } = false;

    // Param: InitialFundUnitValue
    public bool HasInitialFundUnitValue { get; internal set; } = false;
    public bool InitialFundUnitValueIsValid { get; internal set; } = false;
    public decimal? InitialFundUnitValue { get; internal set; } = null;


}
