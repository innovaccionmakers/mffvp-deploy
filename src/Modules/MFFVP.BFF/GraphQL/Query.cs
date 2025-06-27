using Common.SharedKernel.Domain;
using Customers.Presentation.DTOs;
using MFFVP.BFF.Services;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Products.Integrations.Objectives.GetObjectives;
using Products.Presentation.DTOs;
using Products.Presentation.GraphQL;

namespace MFFVP.BFF.GraphQL;

public class Query
{
    //Products Queries

    [GraphQLName("tipoDocumento")]
    public async Task<IReadOnlyCollection<DocumentTypeDto>> GetDocumentTypes([Service] IProductsExperienceQueries productsQueries, CancellationToken cancellationToken)
    {
        return await productsQueries.GetDocumentTypesAsync(cancellationToken);
    }

    [GraphQLName("PortafolioPorObjetivo")]
    public async Task<PortfolioDto> GetPortfolio(string objetiveId, [Service] IProductsExperienceQueries productsQueries, CancellationToken cancellationToken)
    {
        return await productsQueries.GetPortfolioAsync(objetiveId, cancellationToken);
    }

    //Operations Queries
    [GraphQLName("tipoTransaccion")]
    public async Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypes([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetTransactionTypesAsync(cancellationToken);
    }

    [GraphQLName("subtipoTransaccion")]
    public async Task<IReadOnlyCollection<SubTransactionTypeDto>> GetSubTransactionTypes(Guid categoryId, [Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetSubTransactionTypesAsync(categoryId, cancellationToken);
    }

    [GraphQLName("estadoCertificacion")]
    public async Task<IReadOnlyCollection<CertificationStatusDto>> GetCertificationStatuses([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetCertificationStatusesAsync(cancellationToken);
    }

    [GraphQLName("modalidadOrigen")]
    public async Task<IReadOnlyCollection<OriginModeDto>> GetOriginModes([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetOriginModesAsync(cancellationToken);
    }

    [GraphQLName("metodoRecaudo")]

    public async Task<IReadOnlyCollection<CollectionMethodDto>> GetCollectionMethods([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetCollectionMethodsAsync(cancellationToken);
    }

    [GraphQLName("formaPago")]
    public async Task<IReadOnlyCollection<PaymentMethodDto>> GetPaymentMethods([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetPaymentMethodsAsync(cancellationToken);
    }

    [GraphQLName("origen")]

    public async Task<IReadOnlyCollection<OriginContributionDto>> GetOriginContributions([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetOriginContributionsAsync(cancellationToken);
    }

    [GraphQLName("banco")]
    public async Task<IReadOnlyCollection<BankDto>> GetBanks([Service] IOperationsExperienceQueries operationsQueries, CancellationToken cancellationToken)
    {
        return await operationsQueries.GetBanksAsync(cancellationToken);
    }

    [GraphQLName("afilidado")]
    public async Task<IReadOnlyCollection<PeopleDto>> GetAllAssociates(string identificationType,
                                                                       SearchByType? searchBy,
                                                                       string? text,
                                                                       [Service] ExperienceOrchestrator experienceOrchestrator,
                                                                       CancellationToken cancellationToken)
    {
        return await experienceOrchestrator.GetAllAssociatesAsync(identificationType, searchBy, text, cancellationToken);
    }

    [GraphQLName("objetivo")]
    public async Task<IReadOnlyCollection<GoalDto>> GetGoals(string typeId, string identification, StatusType status, [Service] IProductsExperienceQueries productsExperienceQueries, CancellationToken cancellationToken)
    {
        return await productsExperienceQueries.GetGoalsAsync(typeId, identification, status, cancellationToken);
    }
}
