﻿using Associate.Presentation.DTOs;
using Associate.Presentation.GraphQL;
using Closing.Presentation.GraphQL;
using Closing.Presentation.GraphQL.DTOs;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Results;
using Customers.Presentation.DTOs;
using Customers.Presentation.GraphQL;
using HotChocolate.Authorization;
using HotChocolate;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services;
using MFFVP.BFF.Services.Reports;
using Microsoft.AspNetCore.Mvc;
using Operations.Presentation.DTOs;
using Microsoft.Extensions.Logging;
using Operations.Presentation.GraphQL;
using Products.Integrations.Objectives.GetObjectives;
using Products.Presentation.DTOs;
using Products.Presentation.DTOs.PlanFund;
using Reports.Presentation.GraphQL;
using Products.Presentation.GraphQL;
using Treasury.Presentation.DTOs;
using Treasury.Presentation.GraphQL;
using Trusts.Presentation.GraphQL;
using Reports.Domain.DailyClosing;
using Common.SharedKernel.Application.Reports;
using Reports.Domain.TechnicalSheet;

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

    [GraphQLName("obtenerFichaTecnicaPorRangoDeFechaYPortafolio")]
    public async Task<IReadOnlyCollection<TechnicalSheetDto>> GetTechnicalDataSheetByDateRangeAndPortfolio([GraphQLName("idPortafolio")] int portfolioId,
                                                                                                               [GraphQLName("fechaInicio")] DateOnly startDate,
                                                                                                               [GraphQLName("fechaFin")] DateOnly endDate,
                                                                                                               [Service] IProductsExperienceQueries productsQueries,
                                                                                                               CancellationToken cancellationToken)
    {
        return await productsQueries.GetTechnicalSheetsByDateRangeAndPortfolio(startDate, endDate, portfolioId, cancellationToken);
    }

    //Operations Queries
    [GraphQLName("tipoTransaccion")]
    public async Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypes([Service] IOperationsExperienceQueries operationsQueries,
                                                                                   CancellationToken cancellationToken)
    {
        return await operationsQueries.GetTransactionTypesAsync(cancellationToken);
    }

    [GraphQLName("subtipoTransaccion")]
    public async Task<IReadOnlyCollection<OperationTypeDto>> GetSubtransactionTypes([GraphQLName("idTipoTransaccion")] int? categoryId,
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
    public async Task<IReadOnlyCollection<BankDto>> GetBanks([Service] ITreasuryExperienceQueries treasuryQueries,
                                                             CancellationToken cancellationToken)
    {
        return await treasuryQueries.GetBanksAsync(cancellationToken);
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

    [GraphQLName("obtenerValoracionPortafolio")]
    public async Task<IReadOnlyCollection<PortfolioValuationDto>> GetPortfolioValuation([GraphQLName("fechaCierre")] DateOnly closingDate,
                                                                    [Service] IClosingExperienceQueries closingQueries,
                                                                    CancellationToken cancellationToken)
    {
        return await closingQueries.GetPortfolioValuation(closingDate, cancellationToken);
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

    [GraphQLName("generarReporteDepositos")]
    public async Task<GraphqlResult<ReportResponseDto>> GenerateDepositsReportAsync([GraphQLName("processDate")] DateTime processDate,
                                                                   [Service] ReportOrchestrator reportOrchestrator,
                                                                   CancellationToken cancellationToken)
    {
        return await reportOrchestrator.GetReportDataAsync(processDate, ReportType.Deposits, cancellationToken);
    }

    [GraphQLName("generarReporteFichaTecnica")]
    public async Task<GraphqlResult<ReportResponseDto>> GenerateTechnicalSheetReportAsync([GraphQLName("idPortafolio")] int portfolioId,
                                                                                          [GraphQLName("fechaInicio")] DateOnly startDate,
                                                                                          [GraphQLName("fechaFin")] DateOnly endDate,
                                                                                          [Service] ReportOrchestrator reportOrchestrator,
                                                                                          CancellationToken cancellationToken)
    {
        var request = new TechnicalSheetReportRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            PortfolioId = portfolioId
        };
        return await reportOrchestrator.GetReportData(request, ReportType.TechnicalSheet, cancellationToken);
    }

    [GraphQLName("generarReporteSaldosMovimientos")]
    public async Task<GraphqlResult<ReportResponseDto>> GenerateBalancesReportAsync([GraphQLName("fechaInicial")] DateOnly startDate,
                                                                                    [GraphQLName("fechaFinal")] DateOnly endDate,
                                                                                    [GraphQLName("identificacion")] int identificationId,
                                                                                    [Service] ReportOrchestrator reportOrchestrator,
                                                                                    CancellationToken cancellationToken)
    {      
        return await reportOrchestrator.GetReportData((startDate, endDate, identificationId), ReportType.Balances, cancellationToken);
    }

    // Reports Queries
    [GraphQLName("generarReporteCierreDiario")]
    public async Task<GraphqlResult<ReportResponseDto>> GenerateDailyClosingReportAsync(
        [GraphQLName("idPortafolio")] int portfolioId,
        [GraphQLName("fechaGeneracion")] DateTime generationDate,
        [Service] ReportOrchestrator reportOrchestrator,
        CancellationToken cancellationToken)
    {
        var request = new DailyClosingReportRequest
        {
            PortfolioId = portfolioId,
            GenerationDate = generationDate
        };

        return await reportOrchestrator.GetReportData(request, ReportType.DailyClosing, cancellationToken);
    }

    //Trust Queries
    [GraphQLName("getParticipantes")]
    public async Task<int> GetParticipant([GraphQLName("fideicomisoIds")] IEnumerable<long> trustIds, [Service] ITrustExperienceQueries trustQueries)
    {
        return await trustQueries.GetParticipantAsync(trustIds);
    }

}
