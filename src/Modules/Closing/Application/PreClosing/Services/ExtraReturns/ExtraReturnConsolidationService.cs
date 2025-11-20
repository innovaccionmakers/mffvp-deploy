using System.Linq;
using System.Text.Json;

using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Closing.Application.PreClosing.Services.ExtraReturns.Dto;
using Closing.Application.PreClosing.Services.ExtraReturns.Interfaces;
using Closing.Application.PreClosing.Services.Yield.Dto;

using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.ExtraReturns;

public sealed class ExtraReturnConsolidationService(ITrustOperationsLocator trustOperationsLocator)
    : IExtraReturnConsolidationService
{
    public async Task<Result<IReadOnlyCollection<ExtraReturnSummary>>> GetExtraReturnSummariesAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken)
    {
        var operationsResult = await trustOperationsLocator
            .GetTrustOperationsAsync(portfolioId, closingDateUtc, cancellationToken);

        if (operationsResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<ExtraReturnSummary>>(operationsResult.Error);
        }

        if (operationsResult.Value.Count == 0)
        {
            return Result.Failure<IReadOnlyCollection<ExtraReturnSummary>>(ExtraReturnErrors.NoOperationsFound);
        }

        var summaries = operationsResult.Value
            .Select(operation => new ExtraReturnSummary(
                operation.PortfolioId,
                operation.ProcessDateUtc,
                operation.OperationTypeId,
                operation.OperationTypeName,
                operation.Amount,
                JsonSerializer.Serialize(
                    new StringEntityDto(
                        operation.OperationTypeId.ToString(),
                        operation.OperationTypeName))))
            .ToList();

        return Result.Success<IReadOnlyCollection<ExtraReturnSummary>>(summaries);
    }
}

public static class ExtraReturnErrors
{
    public static readonly Error NoOperationsFound = Error.Failure(
        "CLOSING_EXTRA_RETURN_NOT_FOUND",
        "No se encontraron operaciones de rendimientos extraordinarios en Operaciones.");
}
