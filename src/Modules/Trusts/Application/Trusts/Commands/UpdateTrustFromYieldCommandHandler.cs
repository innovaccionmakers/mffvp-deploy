
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.Trusts;
using Trusts.Integrations.TrustYields.Commands;

namespace Trusts.Application.Trusts.Commands;

internal sealed class UpdateTrustFromYieldCommandHandler(
    ITrustRepository trustRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTrustFromYieldCommand>
{
    public async Task<Result> Handle(UpdateTrustFromYieldCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);

        if (trust is null)
        {
            return Result.Failure(new Error(
                "001",
                $"No se encontró el fideicomiso con ID {request.TrustId}.",
                ErrorType.NotFound));
        }

        // Lógica de aplicación del rendimiento
        var newTotalBalance = trust.TotalBalance + request.YieldAmount;

        if (newTotalBalance != request.ClosingBalance)
        {
            return Result.Failure(new Error(
                "002",
                $"Saldo calculado ({newTotalBalance}) no coincide con el saldo de cierre esperado ({request.ClosingBalance}).",
                ErrorType.Validation));
        }

        var expectedCapitalPlusYield = trust.Principal + request.YieldAmount;

        if (newTotalBalance != expectedCapitalPlusYield)
        {
            return Result.Failure(new Error(
                "003",
                $"Nuevo saldo ({newTotalBalance}) no es igual a capital + rendimiento ({expectedCapitalPlusYield}).",
                ErrorType.Validation));
        }

        var newEarnings = trust.Earnings + request.YieldAmount;
        var newEarningsWithholding = trust.EarningsWithholding + request.YieldRetention;
        var newAvailableAmount = newTotalBalance - newEarningsWithholding - trust.ContingentWithholding;

        trust.UpdateDetails(
            trust.AffiliateId,
            trust.ClientOperationId,
            trust.CreationDate,
            trust.ObjectiveId,
            trust.PortfolioId,
            newTotalBalance,
            (int)request.Units,
            trust.Principal,
            newEarnings,
            trust.TaxCondition,
            trust.ContingentWithholding,
            newEarningsWithholding,
            newAvailableAmount,
            trust.Status
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}