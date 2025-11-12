namespace Closing.Domain.TrustYields;
public sealed record TrustYieldCalcInput(
    long TrustId, int PortfolioId, decimal PreClosingBalance, decimal Units, bool isFirstTrustClosingDay, decimal PrevDayClosingBalance, decimal PrevDayUnits);