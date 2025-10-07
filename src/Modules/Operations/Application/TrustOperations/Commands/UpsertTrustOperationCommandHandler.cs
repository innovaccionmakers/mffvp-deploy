using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.Application.Abstractions.Data;
using Operations.Domain.OperationTypes;
using Operations.Domain.TrustOperations;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.Application.TrustOperations.Commands;

internal sealed class UpsertTrustOperationCommandHandler(
    ITrustOperationRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<UpsertTrustOperationCommandHandler> logger)
    : IRequestHandler<UpsertTrustOperationCommand, Result<UpsertTrustOperationResponse>>
{
    private const string ClassName = nameof(UpsertTrustOperationCommandHandler);

    public async Task<Result<UpsertTrustOperationResponse>> Handle(
        UpsertTrustOperationCommand request,
        CancellationToken cancellationToken)
    {

        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var changed = await repository.UpsertAsync(
                portfolioId: request.PortfolioId,
                trustId: request.TrustId,
                processDate: request.ClosingDate,
                amount: request.Amount,
                operationTypeId: request.OperationTypeId,
                clientOperationId: null,
                cancellationToken: cancellationToken);

            long? operationId = null;


            await tx.CommitAsync(cancellationToken);

            return Result.Success(new UpsertTrustOperationResponse(operationId, changed));
        }
        catch (OperationCanceledException)
        {
            try { await tx.RollbackAsync(CancellationToken.None); } catch { }
            throw;
        }
        catch (Exception ex)
        {
            try { await tx.RollbackAsync(CancellationToken.None); } catch { }

            logger.LogError(ex, "{Class} - Error en upsert de operación fideicomiso {TrustId}.", ClassName, request.TrustId);

            return Result.Failure<UpsertTrustOperationResponse>(
                Error.Failure("OPS-UPSERT-ERR", ex.Message));
        }
    }
}
