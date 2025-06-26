using MediatR;
using Products.Presentation.DTOs;

namespace Products.Presentation.GraphQL;

public interface IProductsExperienceQueries
{
    Task<PortfolioDto> GetPortfolioAsync(string objetiveId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DocumentTypeDto>> GetDocumentTypesAsync(      
      CancellationToken cancellationToken = default);

}
