namespace Closing.Domain.TrustYields;

public sealed record TrustYieldCalcInput(
    long TrustId, int PortfolioId, decimal PreClosingBalance, decimal Units);