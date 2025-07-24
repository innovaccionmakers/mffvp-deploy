namespace Trusts.Integrations.DataSync.TrustSync.Response;

public sealed class GetActiveTrustByPortfolioResponse
{
    public long TrustId { get; set; }                     // fideicomiso
    public int PortfolioId { get; set; }
    public decimal TotalBalance { get; set; }             // saldo_precierre
    public decimal Principal { get; set; }                // capital
    public decimal ContingentWithholding { get; set; }    // retención contingente
}
