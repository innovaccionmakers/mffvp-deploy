namespace Operations.Domain.DocumentTypes;

public class DocumentType
{
    public long DocumentTypeId { get; private set; }
    public string Name { get; private set; }
    public string HomologatedCode { get; private set; }
    public bool Status { get; private set; }

    private DocumentType()
    {
    }

    public static DocumentType Create(
        long documentTypeId,
        string name,
        string homologatedCode,
        bool status
    )
    {
        return new DocumentType
        {
            DocumentTypeId = documentTypeId,
            Name = name,
            HomologatedCode = homologatedCode,
            Status = status
        };
    }
}
