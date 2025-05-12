using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.DeleteTrust;

namespace Trusts.Application.Trusts.DeleteTrust;

internal sealed class DeleteTrustCommandHandler(
    ITrustRepository trustRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTrustCommand>
{
    public async Task<Result> Handle(DeleteTrustCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);
        if (trust is null) return Result.Failure(TrustErrors.NotFound(request.TrustId));

        trustRepository.Delete(trust);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}