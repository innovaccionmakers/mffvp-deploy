namespace Operations.Domain.ConfigurationParameters;

public class CollectionMethod
{ 
    public long CollectionMethodId { get; private set; }
    public string Name { get; private set; }
    public string HomologatedCode { get; private set; }
    public bool Status { get; private set; }

    private CollectionMethod()
    {
    }

    public static CollectionMethod Create(
        long collectionMethodId,
        string name,
        string homologatedCode,
        bool status
    )
    {
        return new CollectionMethod
        {
            CollectionMethodId = collectionMethodId,
            Name = name,
            HomologatedCode = homologatedCode,
            Status = status
        };
    }
}
