using Common.SharedKernel.Domain;
using Products.Domain.Commissions;

namespace Products.Domain.AccumulatedCommissions;

public sealed class AccumulatedCommission : Entity
{
    public int AccumulatedCommissionId { get; private set; }
    public int PortfolioId { get; private set; }
    public int CommissionId { get; private set; }
    public decimal AccumulatedValue { get; private set; }
    public decimal PaidValue { get; private set; }
    public decimal PendingValue { get; private set; }
    public DateTime CloseDate { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public DateTime ProcessDate { get; private set; }

    public Commission Commission { get; private set; }

    private AccumulatedCommission()
    {
    }

    public static Result<AccumulatedCommission> Create(
        int portfolioId,
        int commissionId,
        decimal accumulatedValue,
        decimal paidValue,
        decimal pendingValue,
        DateTime closeDate,
        DateTime paymentDate,
        DateTime processDate)
    {
        var accumulatedCommission = new AccumulatedCommission
        {
            PortfolioId = portfolioId,
            CommissionId = commissionId,
            AccumulatedValue = accumulatedValue,
            PaidValue = paidValue,
            PendingValue = pendingValue,
            CloseDate = closeDate,
            PaymentDate = paymentDate,
            ProcessDate = processDate
        };

        return Result.Success(accumulatedCommission);
    }

    public void UpdateValues(
        decimal newAccumulatedValue,
        decimal newPaidValue,
        decimal newPendingValue,
        DateTime newCloseDate,
        DateTime newPaymentDate,
        DateTime newProcessDate)
    {
        AccumulatedValue = newAccumulatedValue;
        PaidValue = newPaidValue;
        PendingValue = newPendingValue;
        CloseDate = newCloseDate;
        PaymentDate = newPaymentDate;
        ProcessDate = newProcessDate;
    }
}