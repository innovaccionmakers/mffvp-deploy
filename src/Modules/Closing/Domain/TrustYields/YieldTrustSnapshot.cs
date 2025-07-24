namespace Closing.Domain.TrustYields;
public sealed record YieldTrustSnapshot
{
    public long TrustId { get; init; }
    public int PortfolioId { get; init; }
    public DateTime ClosingDate { get; init; }
    public decimal PreClosingBalance { get; init; }
    public decimal Capital { get; init; }
    public decimal ContingentRetention { get; init; }
    public DateTime ProcessDate { get; init; }
}
