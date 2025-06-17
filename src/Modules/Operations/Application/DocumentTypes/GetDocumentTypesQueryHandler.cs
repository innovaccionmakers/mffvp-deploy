using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.DocumentTypes;
using Operations.Integrations.DocumentTypes;

namespace Operations.Application.DocumentTypes;

public class GetDocumentTypesQueryHandler(IDocumentTypeRepository documentTypeRepository) : IQueryHandler<GetDocumentTypesQuery, IReadOnlyCollection<DocumentType>>
{
    public async Task<Result<IReadOnlyCollection<DocumentType>>> Handle(GetDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await documentTypeRepository.GetDocumentTypesAsync(cancellationToken);
        return Result.Success(list);
    }
}
