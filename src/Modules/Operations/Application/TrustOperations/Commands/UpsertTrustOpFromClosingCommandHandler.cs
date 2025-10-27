using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.Application.Abstractions.Data;
using Operations.Domain.TrustOperations;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.Application.TrustOperations.Commands;

internal sealed class UpsertTrustOpFromClosingCommandHandler(
    ITrustOperationBulkRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<UpsertTrustOpFromClosingCommandHandler> logger)
    : IRequestHandler<UpsertTrustOpFromClosingCommand, Result<UpsertTrustOpFromClosingResponse>>
{
    private const string ClassName = nameof(UpsertTrustOpFromClosingCommandHandler);

    public async Task<Result<UpsertTrustOpFromClosingResponse>> Handle(
        UpsertTrustOpFromClosingCommand request,
        CancellationToken cancellationToken)
    {
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1) Mapeo desde Closing hacia lo que recibe el repositorio
            var rows = request.TrustYieldOperations
                .Select(op => new TrustYieldOpRowForBulk(
                    TrustId: op.TrustId,
                    OperationTypeId: op.OperationTypeId,
                    Amount: op.Amount,
                    ClientOperationId: op.ClientOperationId,
                    ProcessDateUtc: op.ProcessDateUtc
                ))
                .ToList();

            // 2) Upsert bulk
            var bulkResult = await repository.UpsertBulkAsync(
                portfolioId: request.PortfolioId,
                trustYieldOperations: rows,
                cancellationToken: cancellationToken);

            await tx.CommitAsync(cancellationToken);

            // 3) Response
            var response = new UpsertTrustOpFromClosingResponse(
                Inserted: bulkResult.Inserted,
                Updated: bulkResult.Updated,
                ChangedTrustIds: bulkResult.ChangedTrustIds
            );

            return Result.Success(response);
        }
        catch (OperationCanceledException)
        {
            try { await tx.RollbackAsync(CancellationToken.None); } catch { }
            throw;
        }
        catch (Exception ex)
        {
            try { await tx.RollbackAsync(CancellationToken.None); } catch { }

            logger.LogError(ex,
                "{Class} - Error en upsert bulk de operaciones desde Closing. Portfolio:{PortfolioId} Items:{Count}",
                ClassName, request.PortfolioId, request.TrustYieldOperations?.Count ?? 0);

            return Result.Failure<UpsertTrustOpFromClosingResponse>(
                Error.Failure("OPS-UPSERT-ERR", ex.Message));
        }
    }
}
