using Associate.Presentation.DTOs;
using Associate.Presentation.GraphQL;
using Common.SharedKernel.Domain;
using Customers.Presentation.DTOs;
using Customers.Presentation.GraphQL;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Products.Integrations.Objectives.GetObjectives;
using Products.Presentation.DTOs;
using Products.Presentation.DTOs.PlanFund;
using Products.Presentation.GraphQL;
using Closing.Presentation.GraphQL;
using Closing.Presentation.GraphQL.DTOs;
using Treasury.Presentation.DTOs;
using Treasury.Presentation.GraphQL;
using HotChocolate.Authorization;

namespace MFFVP.BFF.GraphQL;

[Authorize]
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
    public async Task<IReadOnlyCollection<GoalDto>> GetGoals([GraphQLName("idTipoIdentificacion")] string typeId,
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

    [GraphQLName("oficina")]
    public async Task<IReadOnlyCollection<OfficeDto>> GetOffices([Service] IProductsExperienceQueries productsQueries,
                                                                          CancellationToken cancellationToken = default)
    {
        return await productsQueries.GetOfficesAsync(cancellationToken);
    }
    
    [GraphQLName("comercial")]
    public async Task<IReadOnlyCollection<CommercialDto>> GetCommercials([Service] IProductsExperienceQueries productsQueries,
                                                                          CancellationToken cancellationToken = default)
    {
        return await productsQueries.GetCommercialsAsync(cancellationToken);
    }

    [GraphQLName("obtenerPlanFondo")]
    public async Task<PlanFundDto> GetPlanFund([GraphQLName("idAlternativa")] string alternativeId,
                                                 [Service] IProductsExperienceQueries productsQueries,
                                                 CancellationToken cancellationToken)
    {
        return await productsQueries.GetPlanFundAsync(alternativeId, cancellationToken);
    }

    [GraphQLName("objetivosAfiliado")]
    public Task<IReadOnlyCollection<AffiliateGoalDto>> GetAffiliateObjectivesAsync(
        [GraphQLName("idAfiliado")] int affiliateId,
        [Service] IProductsExperienceQueries productsQueries,
        CancellationToken cancellationToken = default)
    {
        return productsQueries.GetAffiliateObjectivesAsync(affiliateId, cancellationToken);
    }

    [GraphQLName("obtenerPortafolios")]
    public async Task<IReadOnlyCollection<PortfolioInformationDto>> GetAllPortfolios([Service] IProductsExperienceQueries productsQueries,
                                                 CancellationToken cancellationToken)
    {
        return await productsQueries.GetAllPortfoliosAsync(cancellationToken);
    }

    //Operations Queries
    [GraphQLName("tipoTransaccion")]
    public async Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypes([Service] IOperationsExperienceQueries operationsQueries,
                                                                                   CancellationToken cancellationToken)
    {
        return await operationsQueries.GetTransactionTypesAsync(cancellationToken);
    }

    [GraphQLName("tipoOperacion")]
    public async Task<IReadOnlyCollection<OperationTypeDto>> GetOperationTypes([GraphQLName("idCategoria")] int? categoryId,
                                                                               [Service] IOperationsExperienceQueries operationsQueries,
                                                                               CancellationToken cancellationToken)
    {
        return await operationsQueries.GetOperationTypesAsync(categoryId, cancellationToken);
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

    //Associates Queries
    [GraphQLName("requisitosPension")]
    public async Task<IReadOnlyCollection<PensionRequirementDto>> GetPensionRequirementsByAssociate([GraphQLName("idAfiliado")] int associateId,
                                                                                                    [Service] IAssociatesExperienceQueries associatesExperienceQueries,
                                                                                                    CancellationToken cancellationToken)
    {
        return await associatesExperienceQueries.GetPensionRequirementsByAssociateAsync(associateId, cancellationToken);
    }


    //Customers Queries
    [GraphQLName("persona")]
    public async Task<IReadOnlyCollection<PersonDto>> GetPersonsByFilter([GraphQLName("tipoIdentificacion")] string? identificationType,
                                                                         [GraphQLName("buscarPor")] SearchByType? searchBy,
                                                                         [GraphQLName("texto")] string? text,
                                                                         [Service] ICustomersExperienceQueries customersQueries,
                                                                         CancellationToken cancellationToken)
    {
        return await customersQueries.GetPersonsByFilter(identificationType, searchBy, text, cancellationToken);
    }

    //Orchestrator Queries
    [GraphQLName("obtenerAfiliadosConFiltros")]
    public async Task<IReadOnlyCollection<AffiliateDto>> GetAllAssociatesByFilter([GraphQLName("tipoIdentificacion")] string? identificationType,
                                                                   [GraphQLName("buscarPor")] SearchByType? searchBy,
                                                                   [GraphQLName("texto")] string? text,
                                                                   [Service] ExperienceOrchestrator experienceOrchestrator,
                                                                   CancellationToken cancellationToken)
    {
        return await experienceOrchestrator.GetAllAssociatesByFilterAsync(identificationType, searchBy, text, cancellationToken);
    }

    [Authorize(Policy = "fvp:associate:activates:view")]
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

    [GraphQLName("movimientosDeTesoreriaPorPortafolios")]
    public async Task<IReadOnlyCollection<TreasuryMovementByPortfoliosDto>> GetTreasuryMovementsByPortfolioIds([GraphQLName("portfolioIds")] IEnumerable<long> portfolioIds,
                                                                                                 [Service] ExperienceOrchestrator experienceOrchestrator,
                                                                                                 CancellationToken cancellationToken)
    {
        return await experienceOrchestrator.GetTreasuryMovementByPortfoliosAsync(portfolioIds, cancellationToken);
    }
            
    //Closing Queries
    [GraphQLName("obtenerPerdidasGanancias")]
    public async Task<ProfitAndLossDto?> GetProfitAndLoss([GraphQLName("idPortafolio")] int portfolioId,
                                                    [GraphQLName("fechaEfectiva")] DateTime effectiveDate,
                                                    [Service] IClosingExperienceQueries closingQueries,
                                                    CancellationToken cancellationToken)
    {
        return await closingQueries.GetProfitAndLossAsync(portfolioId, effectiveDate, cancellationToken);
    }

    //treasury Queries

    [GraphQLName("emisores")]
    public async Task<IReadOnlyCollection<IssuerDto>> GetIssuers([Service] ITreasuryExperienceQueries treasuryQueries,
                                                    CancellationToken cancellationToken)
    {
        return await treasuryQueries.GetIssuersAsync(cancellationToken);
    }

    [GraphQLName("cuentasBancariasPorPortafolio")]
    public async Task<IReadOnlyCollection<BankAccountByPortfolioDto>> GetBankAccountsByPortfolio([GraphQLName("idPortafolio")] long portfolioId,
                                                    [Service] ExperienceOrchestrator experienceOrchestrator,
                                                    CancellationToken cancellationToken)
    {
        return await experienceOrchestrator.GetBankAccountsByPortfolioAsync(portfolioId, cancellationToken);
    }

    [GraphQLName("conceptosTesoreria")]
    public async Task<IReadOnlyCollection<TreasuryConceptDto>> GetTreasuryConcepts([Service] ITreasuryExperienceQueries treasuryQueries,
                                                                                   CancellationToken cancellationToken)
    {
        return await treasuryQueries.GetTreasuryConceptsAsync(cancellationToken);
    }

    [GraphQLName("cuentasBancariasPortafolioEmisor")]
    public async Task<IReadOnlyCollection<BankAccountDto>> GetBankAccountsByPortfolioAndIssuer([GraphQLName("portfolioId")] long portfolioId,
                                                                                [GraphQLName("emisorId")] long issuerId,
                                                                                [Service] ITreasuryExperienceQueries treasuryQueries,
                                                                                CancellationToken cancellationToken)
    {
        return await treasuryQueries.GetBankAccountsByPortfolioAndIssuerAsync(portfolioId, issuerId, cancellationToken);
    }  
}
