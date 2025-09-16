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
using Customers.IntegrationEvents.PersonValidation;
using Products.IntegrationEvents.AdditionalInformation;
using Products.IntegrationEvents.EntityValidation;
using Trusts.IntegrationEvents.GetBalances;
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

    public async Task<Result<GetBalancesByObjectiveResponse>> Handle(
        GetBalancesByObjectiveQuery request,
        CancellationToken cancellationToken)
    {
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

        var personValidation = await rpcClient.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
            new PersonDataRequestEvent(request.DocumentType, request.Identification),
            cancellationToken);

        if (!personValidation.IsValid)
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(personValidation.Code ?? string.Empty, personValidation.Message ?? string.Empty));

        var entityValidation = await rpcClient.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
            new ValidateEntityRequest(request.Entity),
            cancellationToken);

        if (!entityValidation.IsValid)
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(entityValidation.Code ?? string.Empty, entityValidation.Message ?? string.Empty));

        var balancesRpc = await rpcClient.CallAsync<GetBalancesRequest, GetBalancesResponse>(
            new GetBalancesRequest(activation!.ActivateId),
            cancellationToken);

        if (!balancesRpc.Succeeded)
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(balancesRpc.Code ?? string.Empty, balancesRpc.Message ?? string.Empty));

        var balances = balancesRpc.Balances
            .Where(b => b.TotalBalance > 0)
            .ToArray();

        if (balances.Length == 0)
        {
            var emptyPage = new PageInfo(0, 0, request.RecordsPerPage);
            return Result.Success(new GetBalancesByObjectiveResponse(emptyPage, Array.Empty<BalanceByObjectiveItem>()));
        }

        var objectivePortfolioPairs = balances.Select(b => (b.ObjectiveId, b.PortfolioId)).ToArray();
        var additionalInfoRpc = await rpcClient.CallAsync<GetAdditionalInformationRequest, GetAdditionalInformationResponse>(
            new GetAdditionalInformationRequest(activation.ActivateId, objectivePortfolioPairs),
            cancellationToken);

        if (!additionalInfoRpc.Succeeded)
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(additionalInfoRpc.Code ?? string.Empty, additionalInfoRpc.Message ?? string.Empty));

        var additionalInfoLookup = additionalInfoRpc.Items.ToDictionary(i => (i.ObjectiveId, i.PortfolioId));

        var balancesWithInfo = balances
            .Where(b => additionalInfoLookup.ContainsKey((b.ObjectiveId, b.PortfolioId)))
            .ToArray();

        var grouped = balancesWithInfo
            .GroupBy(b => b.ObjectiveId)
            .Select(g =>
            {
                var first = g.First();
                additionalInfoLookup.TryGetValue((first.ObjectiveId, first.PortfolioId), out var info);

                return new BalanceByObjectiveItem(
                    request.Entity,
                    info?.FundCode ?? string.Empty,
                    info?.PortfolioCode ?? string.Empty,
                    info?.FundName ?? string.Empty,
                    info?.ObjectiveName ?? string.Empty,
                    g.Key,
                    request.Identification,
                    string.Empty,
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
        var totalRecords = grouped.Count;
        var totalPages = request.RecordsPerPage <= 0
            ? 0
            : (int)Math.Ceiling(totalRecords / (double)request.RecordsPerPage);

        var paginationContext = new
        {
            request.PageNumber,
            request.RecordsPerPage,
            TotalPages = totalPages
        };

        var (paginationValid, _, paginationErrors) = await ruleEvaluator
            .EvaluateAsync(
                "Associate.BalancesByObjective.PaginationValidation",
                paginationContext,
                cancellationToken);

        if (!paginationValid)
        {
            var first = paginationErrors.First();
            return Result.Failure<GetBalancesByObjectiveResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var itemsPage = request.PageNumber == 0 && request.RecordsPerPage == 0
            ? grouped
            : grouped
                .Skip((request.PageNumber - 1) * request.RecordsPerPage)
                .Take(request.RecordsPerPage)
                .ToList();

        var pageInfo = new PageInfo(totalRecords, totalPages, request.RecordsPerPage);
        var response = new GetBalancesByObjectiveResponse(pageInfo, itemsPage);

        return Result.Success(response);
    }
}
