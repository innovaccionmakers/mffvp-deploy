using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Operations.Domain.ClientOperations;
using Operations.Domain.OperationTypes;
using Operations.Integrations.ClientOperations.GetOperationsND;

namespace Operations.Application.ClientOperations.GetOperationsND;

internal sealed class GetOperationsNDQueryHandler(
    IClientOperationRepository clientOperationRepository,
    IOperationTypeRepository operationTypeRepository,
    IPortfolioLocator portfolioLocator,
    ITrustInfoProvider trustInfoProvider)
    : IQueryHandler<GetOperationsNDQuery, GetOperationsNDResponse>
{
    private const string ContributionOperationName = "Aporte";

    public async Task<Result<GetOperationsNDResponse>> Handle(
        GetOperationsNDQuery request,
        CancellationToken cancellationToken)
    {
        var (normalizedStart, normalizedEnd) = NormalizeDateRange(request.StartDate, request.EndDate);

        if (request.AffiliateId <= 0 || request.ObjectiveId <= 0)
        {
            return Result.Success(CreateEmptyResponse(request));
        }

        var contributionType = await operationTypeRepository
            .GetByNameAsync(ContributionOperationName, cancellationToken);

        if (contributionType is null)
        {
            return Result.Success(CreateEmptyResponse(request));
        }

        var categoryId = checked((int)contributionType.OperationTypeId);

        var categorizedTypes = await operationTypeRepository
            .GetTypesByCategoryAsync(categoryId, cancellationToken);

        if (categorizedTypes.Count == 0)
        {
            return Result.Success(CreateEmptyResponse(request));
        }

        var contributionTypeIds = categorizedTypes
            .Select(type => type.OperationTypeId)
            .Distinct()
            .ToArray();

        var contributionOperations = await clientOperationRepository
            .GetContributionOperationsInRangeAsync(
                contributionTypeIds,
                request.AffiliateId,
                request.ObjectiveId,
                normalizedStart,
                normalizedEnd,
                cancellationToken);

        if (contributionOperations.Count == 0)
        {
            return Result.Success(CreateEmptyResponse(request));
        }

        var portfolios = await ResolvePortfoliosAsync(contributionOperations, cancellationToken);

        if (portfolios.Count == 0)
        {
            return Result.Success(CreateEmptyResponse(request));
        }

        var candidates = FilterByPortfolioDate(contributionOperations, portfolios);

        if (candidates.Count == 0)
        {
            return Result.Success(CreateEmptyResponse(request));
        }

        var eligibleOperations = await FilterByTrustAsync(
            candidates,
            contributionType.Name,
            cancellationToken);

        if (eligibleOperations.Count == 0)
        {
            return Result.Success(CreateEmptyResponse(request));
        }

        var orderedItems = eligibleOperations
            .OrderByDescending(item => item.ProcessDate)
            .ToArray();

        var totalCount = orderedItems.Length;
        var pageSize = NormalizePageSize(request.PageSize, totalCount);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pageNumber = NormalizePageNumber(request.PageNumber);

        if (pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        var skip = (pageNumber - 1) * pageSize;

        var pagedItems = orderedItems
            .Skip(skip)
            .Take(pageSize)
            .ToArray();

        var response = new GetOperationsNDResponse(
            pageNumber,
            pageSize,
            totalCount,
            totalPages,
            pagedItems);

        return Result.Success(response);
    }

    private static (DateTime Start, DateTime End) NormalizeDateRange(DateTime start, DateTime end)
    {
        var normalizedStart = NormalizeToUtc(start).Date;
        var normalizedEnd = NormalizeToUtc(end).Date;

        return normalizedStart <= normalizedEnd
            ? (normalizedStart, normalizedEnd)
            : (normalizedEnd, normalizedStart);
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => value
        };
    }

    private async Task<Dictionary<int, PortfolioSnapshot>> ResolvePortfoliosAsync(
        IReadOnlyCollection<ClientOperation> operations,
        CancellationToken cancellationToken)
    {
        var portfolioIds = operations
            .Select(operation => operation.PortfolioId)
            .Distinct()
            .ToList();

        var tasks = portfolioIds
            .Select(async portfolioId =>
            {
                var result = await portfolioLocator
                    .FindByPortfolioIdAsync(portfolioId, cancellationToken);

                return (portfolioId, result);
            });

        var results = await Task.WhenAll(tasks);

        var snapshots = new Dictionary<int, PortfolioSnapshot>();

        foreach (var (portfolioId, result) in results)
        {
            if (!result.IsSuccess)
            {
                continue;
            }

            var (_, name, currentDate) = result.Value;

            snapshots[portfolioId] = new PortfolioSnapshot(
                name,
                NormalizeToUtc(currentDate));
        }

        return snapshots;
    }

    private static List<(ClientOperation Operation, PortfolioSnapshot Portfolio)> FilterByPortfolioDate(
        IReadOnlyCollection<ClientOperation> operations,
        IReadOnlyDictionary<int, PortfolioSnapshot> portfolios)
    {
        var filtered = new List<(ClientOperation, PortfolioSnapshot)>();

        foreach (var operation in operations)
        {
            if (!portfolios.TryGetValue(operation.PortfolioId, out var snapshot))
            {
                continue;
            }

            var processDate = NormalizeToUtc(operation.ProcessDate).Date;
            var portfolioComparisonDate = snapshot.CurrentDate.AddDays(1).Date;

            if (processDate < portfolioComparisonDate)
            {
                filtered.Add((operation, snapshot));
            }
        }

        return filtered;
    }

    private async Task<List<OperationNdItem>> FilterByTrustAsync(
        IReadOnlyCollection<(ClientOperation Operation, PortfolioSnapshot Portfolio)> candidates,
        string transactionTypeName,
        CancellationToken cancellationToken)
    {
        var resolvedTransactionType = string.IsNullOrWhiteSpace(transactionTypeName)
            ? ContributionOperationName
            : transactionTypeName;

        var tasks = candidates
            .Select(async candidate =>
            {
                var (operation, _) = candidate;
                var trustResult = await trustInfoProvider
                    .GetAsync(operation.ClientOperationId, operation.Amount, cancellationToken);

                return (candidate, trustResult);
            });

        var trustResults = await Task.WhenAll(tasks);

        var eligible = new List<OperationNdItem>();

        foreach (var (candidate, trustResult) in trustResults)
        {
            if (!trustResult.IsSuccess)
            {
                continue;
            }

            var (operation, _) = candidate;

            eligible.Add(new OperationNdItem(
                operation.ClientOperationId,
                NormalizeToUtc(operation.ProcessDate),
                resolvedTransactionType,
                operation.Amount,
                operation.AuxiliaryInformation?.ContingentWithholding ?? 0m));
        }

        return eligible;
    }

    private sealed record PortfolioSnapshot(string Name, DateTime CurrentDate);

    private static GetOperationsNDResponse CreateEmptyResponse(GetOperationsNDQuery request)
    {
        var pageNumber = NormalizePageNumber(request.PageNumber);
        var pageSize = NormalizePageSize(request.PageSize, 0);

        return new GetOperationsNDResponse(pageNumber, pageSize, 0, 0, Array.Empty<OperationNdItem>());
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        return pageNumber > 0 ? pageNumber : 1;
    }

    private static int NormalizePageSize(int pageSize, int fallback)
    {
        if (pageSize > 0)
        {
            return pageSize;
        }

        if (fallback > 0)
        {
            return fallback;
        }

        return 1;
    }
}
