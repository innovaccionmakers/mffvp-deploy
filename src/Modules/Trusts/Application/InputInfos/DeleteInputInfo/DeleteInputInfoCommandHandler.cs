using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.InputInfos;
using Trusts.Integrations.InputInfos.DeleteInputInfo;

namespace Trusts.Application.InputInfos.DeleteInputInfo;

internal sealed class DeleteInputInfoCommandHandler(
    IInputInfoRepository inputinfoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteInputInfoCommand>
{
    public async Task<Result> Handle(DeleteInputInfoCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var inputinfo = await inputinfoRepository.GetAsync(request.InputInfoId, cancellationToken);
        if (inputinfo is null) return Result.Failure(InputInfoErrors.NotFound(request.InputInfoId));

        inputinfoRepository.Delete(inputinfo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}