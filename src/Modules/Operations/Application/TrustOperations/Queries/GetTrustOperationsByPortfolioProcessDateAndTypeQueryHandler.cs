using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.Domain.OperationTypes;
using Operations.Domain.TrustOperations;
using Operations.Integrations.TrustOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Operations.Application.TrustOperations.Queries;

public sealed class GetTrustOperationsByPortfolioProcessDateAndTypeQueryHandler(
    ITrustOperationRepository trustOperationRepository,
    IOperationTypeRepository operationTypeRepository,
    ILogger<GetTrustOperationsByPortfolioProcessDateAndTypeQueryHandler> logger)
    : IQueryHandler<GetTrustOperationsByPortfolioProcessDateAndTypeQuery, IReadOnlyCollection<TrustOperationResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrustOperationResponse>>> Handle(
        GetTrustOperationsByPortfolioProcessDateAndTypeQuery request,
        CancellationToken cancellationToken)
    {
        var operations = await trustOperationRepository.GetByPortfolioProcessDateAndOperationTypeAsync(
            request.PortfolioId,
            request.ProcessDate,
            request.OperationTypeId,
            cancellationToken);

        if (operations.Count == 0)
        {
            return Result.Success<IReadOnlyCollection<TrustOperationResponse>>(Array.Empty<TrustOperationResponse>());
        }

        var operationType = await operationTypeRepository.GetByIdAsync(
            request.OperationTypeId,
            cancellationToken);

        if (operationType is null)
        {
            logger.LogWarning(
                "No se encontró el tipo de operación con id {OperationTypeId}.",
                request.OperationTypeId);
        }

        var operationTypeSummary = new TrustOperationTypeSummary(
            request.OperationTypeId,
            operationType?.Name ?? string.Empty);

        var items = operations
            .Select(operation => new TrustOperationResponse(
                operation.PortfolioId,
                operation.ProcessDate,
                operationTypeSummary,
                operation.Amount))
            .ToList();

        return Result.Success((IReadOnlyCollection<TrustOperationResponse>)items);
    }
}
