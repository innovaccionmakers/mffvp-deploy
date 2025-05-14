using Common.SharedKernel.Domain;
using Trusts.Domain.Trusts;

namespace Trusts.Domain.TrustHistories;

public sealed class TrustHistory : Entity
{
    private TrustHistory()
    {
    }

    public long TrustHistoryId { get; private set; }
    public long TrustId { get; private set; }
    public decimal Earnings { get; private set; }
    public DateTime Date { get; private set; }
    public string SalesUserId { get; private set; }

    public static Result<TrustHistory> Create(
        decimal earnings, DateTime date, string salesUserId, Trust trust
    )
    {
        var trusthistory = new TrustHistory
        {
            TrustHistoryId = default,

            TrustId = trust.TrustId,
            Earnings = earnings,
            Date = date,
            SalesUserId = salesUserId
        };

        trusthistory.Raise(new TrustHistoryCreatedDomainEvent(trusthistory.TrustHistoryId));
        return Result.Success(trusthistory);
    }

    public void UpdateDetails(
        long newTrustId, decimal newEarnings, DateTime newDate, string newSalesUserId
    )
    {
        TrustId = newTrustId;
        Earnings = newEarnings;
        Date = newDate;
        SalesUserId = newSalesUserId;
    }
}