namespace Operations.Domain.DocumentTypes;

public interface IDocumentTypeRepository
{
    Task<IReadOnlyCollection<DocumentType>> GetDocumentTypesAsync(CancellationToken cancellationToken = default);
}
