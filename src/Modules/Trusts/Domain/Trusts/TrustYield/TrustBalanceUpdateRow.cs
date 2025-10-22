
namespace Trusts.Domain.Trusts.TrustYield;

// POCO liviano para BulkRead/BulkUpdate de saldos
public sealed class TrustBalanceUpdateRow
{
    public long TrustId { get; set; }               
    public decimal TotalBalance { get; set; }          // saldo_total
    public decimal Earnings { get; set; }              // rendimiento
    public decimal EarningsWithholding { get; set; }   // retencion_rendimiento
    public decimal ContingentWithholding { get; set; } // retencion_contingente
    public decimal AvailableAmount { get; set; }       // disponible
    public decimal Principal { get; set; }             // capital
}