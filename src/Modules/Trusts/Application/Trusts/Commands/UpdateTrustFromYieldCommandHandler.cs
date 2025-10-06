
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.Trusts;
using Trusts.Integrations.TrustYields.Commands;

namespace Trusts.Application.Trusts.Commands;

internal sealed class UpdateTrustFromYieldCommandHandler(
    ITrustRepository trustRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateTrustFromYieldCommandHandler> logger)
    : ICommandHandler<UpdateTrustFromYieldCommand>
{
    private const string ClassName = nameof(UpdateTrustFromYieldCommandHandler);

    public async Task<Result> Handle(UpdateTrustFromYieldCommand request, CancellationToken cancellationToken)
    {

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


        logger.LogInformation("{Class} - Intentando actualización. TrustId:{TrustId} ClosingDate:{ClosingDate}", ClassName, request.TrustId, request.ClosingDate);

        var affectedRows = await trustRepository.TryApplyYieldToBalanceAsync(
            request.TrustId,
            request.YieldAmount,
            request.YieldRetention,
            request.ClosingBalance,
            cancellationToken);

        if (affectedRows == 1)
        {
            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }

        return Result.Failure(new Error("ERR001", "No se pudo actualizar Fideicomiso.", ErrorType.Validation));
     
    }
}