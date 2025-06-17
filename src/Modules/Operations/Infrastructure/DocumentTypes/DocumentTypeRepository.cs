using Microsoft.EntityFrameworkCore;
using Operations.Domain.DocumentTypes;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.DocumentTypes;

public class DocumentTypeRepository(OperationsDbContext context) : IDocumentTypeRepository
{
    const string ConfigurationParameterType = "TipoDocumento";
    public async Task<IReadOnlyCollection<DocumentType>> GetDocumentTypesAsync(CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationParameters
            .Where(dt => dt.Status && dt.Type == ConfigurationParameterType)
            .Select(dt => DocumentType.Create(
                dt.ConfigurationParameterId,
                dt.HomologationCode,
                dt.Name,
                dt.Status
            )).ToListAsync(cancellationToken);
    }
}
