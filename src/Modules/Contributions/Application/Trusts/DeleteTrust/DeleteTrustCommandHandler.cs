using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.Trusts;
using Contributions.Integrations.Trusts.DeleteTrust;
using Contributions.Application.Abstractions.Data;

namespace Contributions.Application.Trusts.DeleteTrust;

internal sealed class DeleteTrustCommandHandler(
    ITrustRepository trustRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTrustCommand>
{
    public async Task<Result> Handle(DeleteTrustCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);
        if (trust is null)
        {
            return Result.Failure(TrustErrors.NotFound(request.TrustId));
        }
        
        trustRepository.Delete(trust);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}