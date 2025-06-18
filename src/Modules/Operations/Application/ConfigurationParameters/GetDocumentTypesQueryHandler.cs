using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetDocumentTypesQueryHandler(IConfigurationParameterRepository repository) : IQueryHandler<GetDocumentTypesQuery, IReadOnlyCollection<DocumentType>>
{
    public async Task<Result<IReadOnlyCollection<DocumentType>>> Handle(GetDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetDocumentTypesAsync(cancellationToken);
        return Result.Success(list);
    }
}
