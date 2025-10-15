using Associate.Application.Abstractions;
using Associate.Domain.Activates;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.Balances.GetBalancesByObjective;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Customers.IntegrationEvents.PersonInformation;
using Products.IntegrationEvents.AdditionalInformation;
using Products.IntegrationEvents.EntityValidation;
using Trusts.IntegrationEvents.GetBalances;
using System.Collections.Generic;
using System.Linq;

namespace Associate.Application.Balances.GetBalancesByObjective;

internal sealed class GetBalancesByObjectiveHandler(
    IActivateRepository activateRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IRpcClient rpcClient)
    : IQueryHandler<GetBalancesByObjectiveQuery, GetBalancesByObjectiveResponse>
{
    private const string ValidationWorkflow = "Associate.BalancesByObjective.Validation";
    private const string PaginationValidationWorkflow = "Associate.BalancesByObjective.PaginationValidation";

    public async Task<Result<GetBalancesByObjectiveResponse>> Handle(
        GetBalancesByObjectiveQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumberProvided = request.PageNumber.HasValue;
        var recordsPerPageProvided = request.RecordsPerPage.HasValue;

        var pageNumber = request.PageNumber ?? 0;
        var recordsPerPage = request.RecordsPerPage ?? 0;

        var earlyPaginationContext = new
        {
            PageNumberProvided = pageNumberProvided,
            RecordsPerPageProvided = recordsPerPageProvided,
            PageNumber = pageNumber,
            RecordsPerPage = recordsPerPage,
            TotalPages = int.MaxValue
        };

        var (earlyPaginationValid, _, earlyPaginationErrors) = await ruleEvaluator
            .EvaluateAsync(
                PaginationValidationWorkflow,
                earlyPaginationContext,
                cancellationToken);

        if (!earlyPaginationValid)
        {
            var first = earlyPaginationErrors.First();
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var documentType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.DocumentType,
            HomologScope.Of<GetBalancesByObjectiveQuery>(q => q.DocumentType),
            cancellationToken);

        Activate? activation = null;
        if (documentType is not null)
            activation = await activateRepository.GetByIdTypeAndNumber(
                documentType.Uuid,
                request.Identification,
                cancellationToken);

        var validationContext = new
        {
            DocumentTypeProvided = !string.IsNullOrWhiteSpace(request.DocumentType),
            DocumentTypeExists = documentType is not null,
            IdentificationProvided = !string.IsNullOrWhiteSpace(request.Identification),
            ActivateExists = activation is not null,
            EntityProvided = !string.IsNullOrWhiteSpace(request.Entity)
        };

        var (valid, _, errors) = await ruleEvaluator
            .EvaluateAsync(ValidationWorkflow, validationContext, cancellationToken);

        if (!valid)
        {
            var first = errors.First();
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var requiredDocumentType = request.DocumentType!;
        var requiredIdentification = request.Identification!;
        var requiredEntity = request.Entity!;
        var requiredActivation = activation!;

        var personInformation = await rpcClient.CallAsync<GetPersonInformationRequest, GetPersonInformationResponse>(
            new GetPersonInformationRequest(requiredDocumentType, requiredIdentification),
            cancellationToken);

        if (!personInformation.IsValid)
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(personInformation.Code ?? string.Empty, personInformation.Message ?? string.Empty));

        var entityValidation = await rpcClient.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
            new ValidateEntityRequest(requiredEntity),
            cancellationToken);

        if (!entityValidation.IsValid)
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(entityValidation.Code ?? string.Empty, entityValidation.Message ?? string.Empty));

        var balancesRpc = await rpcClient.CallAsync<GetBalancesRequest, GetBalancesResponse>(
            new GetBalancesRequest(requiredActivation.ActivateId),
            cancellationToken);

        if (!balancesRpc.Succeeded)
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(balancesRpc.Code ?? string.Empty, balancesRpc.Message ?? string.Empty));

        var balances = balancesRpc.Balances
            .Where(b => b.TotalBalance > 0)
            .ToArray();

        List<BalanceByObjectiveItem> grouped;
        if (balances.Length == 0)
        {
            grouped = new List<BalanceByObjectiveItem>(0);
        }
        else
        {
            var objectivePortfolioPairs = balances.Select(b => (b.ObjectiveId, b.PortfolioId)).ToArray();
            var additionalInfoRpc = await rpcClient.CallAsync<GetAdditionalInformationRequest, GetAdditionalInformationResponse>(
                new GetAdditionalInformationRequest(requiredActivation.ActivateId, objectivePortfolioPairs),
                cancellationToken);

            if (!additionalInfoRpc.Succeeded)
                return Result.Failure<GetBalancesByObjectiveResponse>(
                    Error.Validation(additionalInfoRpc.Code ?? string.Empty, additionalInfoRpc.Message ?? string.Empty));

            var additionalInfoLookup = additionalInfoRpc.Items.ToDictionary(i => (i.ObjectiveId, i.PortfolioId));

            var balancesWithInfo = balances
                .Where(b => additionalInfoLookup.ContainsKey((b.ObjectiveId, b.PortfolioId)))
                .ToArray();

            grouped = balancesWithInfo
                .GroupBy(b => b.ObjectiveId)
                .Select(g =>
                {
                    var first = g.First();
                    additionalInfoLookup.TryGetValue((first.ObjectiveId, first.PortfolioId), out var info);

                    return new BalanceByObjectiveItem(
                        requiredEntity,
                        info?.FundCode ?? string.Empty,
                        info?.PortfolioCode ?? string.Empty,
                        info?.FundName ?? string.Empty,
                        info?.ObjectiveName ?? string.Empty,
                        g.Key,
                        requiredIdentification,
                        personInformation.Person?.FullName ?? string.Empty,
                        Math.Round(g.Sum(x => x.AvailableAmount), 2),
                        Math.Round(g.Sum(x => x.TotalBalance), 2),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        "N",
                        "Kit Capital");
                })
                .ToList();
        }
        var totalRecords = grouped.Count;
        var noPaginationProvided = !pageNumberProvided && !recordsPerPageProvided;

        var effectivePageNumber = noPaginationProvided
            ? (totalRecords == 0 ? 0 : 1)
            : pageNumber;

        var effectiveRecordsPerPage = noPaginationProvided
            ? (totalRecords == 0 ? 0 : totalRecords)
            : recordsPerPage;

        var totalPages = effectiveRecordsPerPage <= 0
            ? 0
            : (int)Math.Ceiling(totalRecords / (double)effectiveRecordsPerPage);

        var paginationContext = new
        {
            PageNumberProvided = pageNumberProvided,
            RecordsPerPageProvided = recordsPerPageProvided,
            PageNumber = effectivePageNumber,
            RecordsPerPage = effectiveRecordsPerPage,
            TotalPages = totalPages
        };

        var (paginationValid, _, paginationErrors) = await ruleEvaluator
            .EvaluateAsync(
                PaginationValidationWorkflow,
                paginationContext,
                cancellationToken);

        if (!paginationValid)
        {
            var first = paginationErrors.First();
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var itemsPage = noPaginationProvided || effectiveRecordsPerPage <= 0
            ? grouped
            : grouped
                .Skip((effectivePageNumber - 1) * effectiveRecordsPerPage)
                .Take(effectiveRecordsPerPage)
                .ToList();

        var pageInfo = new PageInfo(totalRecords, totalPages, effectiveRecordsPerPage);
        var response = new GetBalancesByObjectiveResponse(pageInfo, itemsPage);

        return Result.Success(response);
    }
}
