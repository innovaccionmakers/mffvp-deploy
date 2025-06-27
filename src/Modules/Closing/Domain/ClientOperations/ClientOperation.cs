using Common.SharedKernel.Domain;

namespace Closing.Domain.ClientOperations;

public sealed class ClientOperation : Entity
{
    public long ClientOperationId { get; private set; }
    public DateTime FilingDate { get; private set; }
    public int AffiliateId { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public long TransactionSubtypeId { get; private set; }
    public DateTime ApplicationDate { get; private set; }

    private ClientOperation()
    {
    }

    public static Result<ClientOperation> Create(
        DateTime filingDate,
        int affiliateId,
        int objectiveId,
        int portfolioId,
        decimal amount,
        DateTime processDate,
        long transactionSubtypeId,
        DateTime applicationDate)
    {
        var clientOperation = new ClientOperation
        {
            ClientOperationId = default,
            FilingDate = filingDate,
            AffiliateId = affiliateId,
            ObjectiveId = objectiveId,
            PortfolioId = portfolioId,
            Amount = amount,
            ProcessDate = processDate,
            TransactionSubtypeId = transactionSubtypeId,
            ApplicationDate = applicationDate
        };

        return Result.Success(clientOperation);
    }

    public void UpdateDetails(
        DateTime filingDate,
        int affiliateId,
        int objectiveId,
        int portfolioId,
        decimal amount,
        DateTime processDate,
        long transactionSubtypeId,
        DateTime applicationDate)
    {
        FilingDate = filingDate;
        AffiliateId = affiliateId;
        ObjectiveId = objectiveId;
        PortfolioId = portfolioId;
        Amount = amount;
        ProcessDate = processDate;
        TransactionSubtypeId = transactionSubtypeId;
        ApplicationDate = applicationDate;
    }
}