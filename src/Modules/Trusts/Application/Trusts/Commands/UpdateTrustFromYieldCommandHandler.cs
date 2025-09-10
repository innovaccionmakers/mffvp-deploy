
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
        logger.LogInformation(
              "{Class} - Inicio. TrustId:{TrustId}, FechaCierre:{ClosingDate}, Rendimiento:{YieldAmount}, Retención:{YieldRetention}, SaldoCierreEsperado:{ClosingBalance}",
              ClassName, request.TrustId, request.ClosingDate, request.YieldAmount, request.YieldRetention, request.ClosingBalance);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


        logger.LogInformation("{Class} - Intentando actualización. TrustId:{TrustId} ClosingDate:{ClosingDate}", ClassName, request.TrustId, request.ClosingDate);

        var affectedRows = await trustRepository.TryApplyYieldSetBasedAsync(
            request.TrustId,
            request.YieldAmount,
            request.YieldRetention,
            request.ClosingBalance,
            cancellationToken);

        logger.LogInformation(
            "{Class} - Actualización completada. FilasAfectadas:{AffectedRows}, TrustId:{TrustId} ClosingDate:{ClosingDate}",
            ClassName, affectedRows, request.TrustId, request.ClosingDate);

        if (affectedRows == 1)
        {
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("{Class} - Éxito. Fideicomiso actualizado. TrustId:{TrustId}  ClosingDate:{ClosingDate}", ClassName, request.TrustId, request.ClosingDate);
            var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);
            if (trust is not null)
            {
                logger.LogInformation(
                    "{Class} - Detalles fideicomiso actualizado. ClosingDate:{ClosingDate}. TrustId:{TrustId}, " +
                    "TotalBalance:{TotalBalance}, CapitalPrincipal:{CapitalPrincipal}, Earnings:{Earnings}, " +
                    "EarningsWithholding:{EarningsWithholding}, ContingentWithholding:{ContingentWithholding}, " +
                    "AvailableAmount:{AvailableAmount}",
                    ClassName,
                    request.ClosingDate,
                    trust.TrustId,
                    trust.TotalBalance,
                    trust.Principal,
                    trust.Earnings,
                    trust.EarningsWithholding,
                    trust.ContingentWithholding,
                    trust.AvailableAmount);
            }
            return Result.Success();
        }

         await transaction.RollbackAsync(cancellationToken);

        logger.LogError(
            "{Class} - Puede haberse dado Concurrencia. Las condiciones pasan pero la actualización afectó 0 filas. FideicomisoId:{TrustId} ClosingDate:{ClosingDate}",
            ClassName, request.TrustId, request.ClosingDate);

        return Result.Failure(new Error("CONC001", "No se pudo actualizar por concurrencia.", ErrorType.Validation));
     
    }
}