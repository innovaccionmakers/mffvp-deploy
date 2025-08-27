using Closing.Application.PreClosing.Services.Validation.Dto;

namespace Closing.Application.PreClosing.Services.Validation.Context;

public sealed class PreclosingValidationContextBuilder
{
    private readonly PreclosingValidationContext _ctx = new();

    private bool _closingDateSet;
    private bool _currentDateSet;
    private bool _firstDayStateSet; 

    public PreclosingValidationContextBuilder WithClosingDate(DateOnly date)
    {
        _ctx.ClosingDate = date;
        _closingDateSet = true;
        return this;
    }

    public PreclosingValidationContextBuilder WithCurrentDate(DateOnly date)
    {
        _ctx.CurrentDate = date;
        _currentDateSet = true;
        return this;
    }

    /// <summary>Mapea todo el resultado de EvaluateFirstDayStateAsync.</summary>
    public PreclosingValidationContextBuilder WithFirstDayState(FirstDayStateResult state)
    {
        _ctx.IsFirstClosingDay = state.IsFirstDay;
        _ctx.ProfitAndLossExists = state.HasPandL;
        _ctx.TreasuryMovementsExists = state.HasTreasury;
        _ctx.HasClientOperations = state.HasClientOps;
        _ctx.HasInitialFundUnitValue = state.HasInitialFundUnitValue;
        _ctx.InitialFundUnitValueIsValid = state.IsInitialFundUnitValueValid;
        _ctx.InitialFundUnitValue = state.InitialFundUnitValue;
        _firstDayStateSet = true;
        return this;
    }

    /// <summary>Datos de Comisión Administrativa (opcionales).</summary>
    public PreclosingValidationContextBuilder WithAdminCommission(int count, bool isNumber, bool between0And100)
    {
        _ctx.AdminCommissionCount = count;
        _ctx.AdminCommissionIsNumber = isNumber;
        _ctx.AdminCommissionBetween0And100 = between0And100;
        return this;
    }

    /// <summary>Indica si ya existe un cierre generado para la fecha (opcional).</summary>
    public PreclosingValidationContextBuilder WithClosingGenerated(bool exists)
    {
        _ctx.ExistsClosingGenerated = exists;
        return this;
    }

    public PreclosingValidationContext Build()
    {
        if (!_closingDateSet || !_currentDateSet)
            throw new InvalidOperationException("ClosingDate and CurrentDate are required to build PreclosingValidationContext.");
        return _ctx;
    }
}
