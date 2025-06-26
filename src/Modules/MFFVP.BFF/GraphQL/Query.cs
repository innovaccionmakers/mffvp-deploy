using Associate.Presentation.DTOs;
using Associate.Presentation.GraphQL;
using Common.SharedKernel.Domain;
using Customers.Presentation.DTOs;
using Customers.Presentation.GraphQL;
using MFFVP.BFF.Services;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Products.Presentation.DTOs;
using Products.Presentation.GraphQL;

namespace MFFVP.BFF.GraphQL;

public class Query
{
    //Products Queries
    public async Task<IReadOnlyCollection<DocumentTypeDto>> GetDocumentTypes([Service] IProductsExperienceQueries productsQueries, CancellationToken cancellationToken)
    {
        return await productsQueries.GetDocumentTypesAsync(cancellationToken);
    }

    public async Task<PortfolioDto> GetPortfolio(string objetiveId, [Service] IProductsExperienceQueries productsQueries, CancellationToken cancellationToken)
    {
        return await productsQueries.GetPortfolioAsync(objetiveId, cancellationToken);
    }

    //Operations Queries
    public async Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypes([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetTransactionTypesAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<SubTransactionTypeDto>> GetSubTransactionTypes(Guid categoryId, [Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetSubTransactionTypesAsync(categoryId, cancellationToken);
    }
    public async Task<IReadOnlyCollection<CertificationStatusDto>> GetCertificationStatuses([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetCertificationStatusesAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<OriginModeDto>> GetOriginModes([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetOriginModesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CollectionMethodDto>> GetCollectionMethods([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetCollectionMethodsAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PaymentMethodDto>> GetPaymentMethods([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetPaymentMethodsAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OriginContributionDto>> GetOriginContributions([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetOriginContributionsAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BankDto>> GetBanks([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetBanksAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PeopleDto>> GetAllAssociates(string identificationType,
                                                                       SearchByType? searchBy,
                                                                       string? text,
                                                                       [Service] ExperienceOrchestrator experienceOrchestrator,
                                                                       CancellationToken cancellationToken)
    {
        return await experienceOrchestrator.GetAllAssociatesAsync(identificationType, searchBy, text, cancellationToken);
    }
}
