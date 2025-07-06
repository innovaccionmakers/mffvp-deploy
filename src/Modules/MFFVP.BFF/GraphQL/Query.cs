using Common.SharedKernel.Domain;
using Customers.Presentation.DTOs;
using Customers.Presentation.GraphQL;
using MFFVP.BFF.DTOs;
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
    public async Task<IReadOnlyCollection<DocumentTypeDto>> GetDocumentTypes([Service] IProductsExperienceQueries productsQueries,
                                                                             CancellationToken cancellationToken)
    {
        return await productsQueries.GetDocumentTypesAsync(cancellationToken);
    }

    [GraphQLName("portafolio")]
    public async Task<PortfolioDto> GetPortfolio([GraphQLName("idObjetivo")] string objetiveId,
                                                 [Service] IProductsExperienceQueries productsQueries,
                                                 CancellationToken cancellationToken)
    {
        return await productsQueries.GetPortfolioAsync(objetiveId, cancellationToken);
    }

    [GraphQLName("objetivo")]
    public async Task<IReadOnlyCollection<GoalDto>> GetGoals([GraphQLName("idTipo")] string typeId,
                                                         [GraphQLName("identificacion")] string identification,
                                                         [GraphQLName("estado")] StatusType status,
                                                         [Service] IProductsExperienceQueries productsExperienceQueries,
                                                         CancellationToken cancellationToken)
    {
        return await productsExperienceQueries.GetGoalsAsync(typeId, identification, status, cancellationToken);
    }

    [GraphQLName("alternativa")]
    public async Task<IReadOnlyCollection<AlternativeDto>> GetAlternatives([Service] IProductsExperienceQueries productsQueries,
                                                                           CancellationToken cancellationToken)
    {
        return await productsQueries.GetAlternativesAsync(cancellationToken);
    }

    [GraphQLName("tipoObjetivo")]
    public async Task<IReadOnlyCollection<GoalTypeDto>> GetGoalTypesAsync([Service] IProductsExperienceQueries productsQueries,
                                                                          CancellationToken cancellationToken = default)
    {
        return await productsQueries.GetGoalTypesAsync(cancellationToken);
    }

    //Operations Queries
    [GraphQLName("tipoTransaccion")]
    public async Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypes([Service] IOperationsExperienceQueries operationsQueries,
                                                                                   CancellationToken cancellationToken)
    {
        return await operationsQueries.GetTransactionTypesAsync(cancellationToken);
    }

    [GraphQLName("subtipoTransaccion")]
    public async Task<IReadOnlyCollection<SubTransactionTypeDto>> GetSubTransactionTypes([GraphQLName("idCategoria")] Guid categoryId,
                                                                                         [Service] IOperationsExperienceQueries operationsQueries,
                                                                                         CancellationToken cancellationToken)
    {
        return await operationsQueries.GetSubTransactionTypesAsync(categoryId, cancellationToken);
    }

    [GraphQLName("estadoCertificacion")]
    public async Task<IReadOnlyCollection<CertificationStatusDto>> GetCertificationStatuses([Service] IOperationsExperienceQueries operationsQueries,
                                                                                            CancellationToken cancellationToken)
    {
        return await operationsQueries.GetCertificationStatusesAsync(cancellationToken);
    }

    [GraphQLName("modalidadOrigen")]
    public async Task<IReadOnlyCollection<OriginModeDto>> GetOriginModes([GraphQLName("idOrigen")] int originId,
                                                                         [Service] IOperationsExperienceQueries operationsQueries,
                                                                         CancellationToken cancellationToken)
    {
        return await operationsQueries.GetOriginModesAsync(originId, cancellationToken);
    }

    [GraphQLName("metodoRecaudo")]

    public async Task<IReadOnlyCollection<CollectionMethodDto>> GetCollectionMethods([Service] IOperationsExperienceQueries operationsQueries,
                                                                                     CancellationToken cancellationToken)
    {
        return await operationsQueries.GetCollectionMethodsAsync(cancellationToken);
    }

    [GraphQLName("formaPago")]
    public async Task<IReadOnlyCollection<PaymentMethodDto>> GetPaymentMethods([Service] IOperationsExperienceQueries operationsQueries,
                                                                               CancellationToken cancellationToken)
    {
        return await operationsQueries.GetPaymentMethodsAsync(cancellationToken);
    }

    [GraphQLName("origen")]

    public async Task<IReadOnlyCollection<OriginContributionDto>> GetOriginContributions([Service] IOperationsExperienceQueries operationsQueries,
                                                                                         CancellationToken cancellationToken)
    {
        return await operationsQueries.GetOriginContributionsAsync(cancellationToken);
    }

    [GraphQLName("banco")]
    public async Task<IReadOnlyCollection<BankDto>> GetBanks([Service] IOperationsExperienceQueries operationsQueries,
                                                             CancellationToken cancellationToken)
    {
        return await operationsQueries.GetBanksAsync(cancellationToken);
    }

    [GraphQLName("retencionContingente")]
    public async Task<string> GetWithholdingContingency([Service] IOperationsExperienceQueries operationsQueries,
                                                                             CancellationToken cancellationToken)
    {
        return await operationsQueries.GetWithholdingContingencyAsync(cancellationToken);
    }

    //Customers Queries
    [GraphQLName("persona")]
    public async Task<IReadOnlyCollection<PersonDto>> GetPersonsByFilter([GraphQLName("tipoIdentificacion")] string identificationType,
                                                                         [GraphQLName("buscarPor")] SearchByType? searchBy,
                                                                         [GraphQLName("texto")] string? text,
                                                                         [Service] ICustomersExperienceQueries customersQueries,
                                                                         CancellationToken cancellationToken)
    {
        return await customersQueries.GetPersonsByFilter(identificationType, searchBy, text, cancellationToken);
    }

    //Orchestrator Queries

    [GraphQLName("obtenerAfiliadosConFiltros")]
    public async Task<IReadOnlyCollection<AffiliateDto>> GetAllAssociatesByFilter([GraphQLName("tipoIdentificacion")] string identificationType,
                                                                   [GraphQLName("buscarPor")] SearchByType? searchBy,
                                                                   [GraphQLName("texto")] string? text,
                                                                   [Service] ExperienceOrchestrator experienceOrchestrator,
                                                                   CancellationToken cancellationToken)
    {
        return await experienceOrchestrator.GetAllAssociatesByFilterAsync(identificationType, searchBy, text, cancellationToken);
    }

    [GraphQLName("obtenerAfiliados")]
    public async Task<IReadOnlyCollection<AffiliateDto>> GetAllAssociates([Service] ExperienceOrchestrator experienceOrchestrator,
                                                                          CancellationToken cancellationToken)
    {
        return await experienceOrchestrator.GetAllAssociatesAsync(cancellationToken);
    }

    [GraphQLName("afilidadoPorId")]
    public async Task<AffiliateDto?> GetAffiliateById([GraphQLName("idAfiliado")] int affiliateId,
                                                     [Service] ExperienceOrchestrator experienceOrchestrator,
                                                     CancellationToken cancellationToken)
    {
        return await experienceOrchestrator.GetAssociateByIdAsync(affiliateId, cancellationToken);
    }
}
