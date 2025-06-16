using Operations.Domain.SubtransactionTypes;

namespace Operations.Domain.TransactionTypes;

public class TransactionType
{
    public long TransactionTypeId { get; private set; }
    public string Name { get; private set; }
    public string HomologatedCode { get; private set; }
    public bool Status { get; private set; }
    public IReadOnlyCollection<SubtransactionType> SubtransactionTypes { get; private set; } = [];
    private TransactionType()
    {
    }

    public static TransactionType Create(
        long transactionTypeId,
        string name,
        string homologatedCode,
        bool status,
        IReadOnlyCollection<SubtransactionType> subtransactionTypes
    )
    {
        return new TransactionType
        {
            TransactionTypeId = transactionTypeId,
            Name = name,
            HomologatedCode = homologatedCode,
            Status = status,
            SubtransactionTypes = subtransactionTypes
        };        
    }

}
