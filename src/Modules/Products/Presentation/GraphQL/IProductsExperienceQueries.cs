using Products.Integrations.Objectives.GetObjectives;
using Products.Presentation.DTOs;

namespace Products.Presentation.GraphQL;

public interface IProductsExperienceQueries
{
    Task<PortfolioDto> GetPortfolioAsync(string objetiveId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DocumentTypeDto>> GetDocumentTypesAsync(      
      CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AlternativeDto>> GetAlternativesAsync(
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<GoalTypeDto>> GetGoalTypesAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<GoalDto>> GetGoalsAsync(
        string typeId, string identification, StatusType status,
        CancellationToken cancellationToken = default);
}
