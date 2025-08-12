using Products.Integrations.Objectives.GetObjectives;
using Products.Presentation.DTOs;
using Products.Presentation.DTOs.PlanFund;

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
    Task<IReadOnlyCollection<OfficeDto>> GetOfficesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CommercialDto>> GetCommercialsAsync(
        CancellationToken cancellationToken = default);

    Task<PlanFundDto> GetPlanFundAsync(string alternativeId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AffiliateGoalDto>> GetAffiliateObjectivesAsync(
        int affiliateId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PortfolioInformationDto>> GetAllPortfoliosAsync(
        CancellationToken cancellationToken = default);

    Task<PortfolioInformationDto?> GetPortfolioByIdAsync(long portfolioId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PortfolioInformationDto>> GetPortfoliosByIdsAsync(IEnumerable<long> portfolioIds, CancellationToken cancellationToken = default);
}
