using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.RolePermissions;
using Security.Domain.RolePermissions;


namespace Security.Application.RolePermissions;

public sealed record DeleteRolePermissionCommandHandler(
    IRolePermissionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteRolePermissionCommand>
{
    public async Task<Result> Handle(DeleteRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await repository.GetAsync(request.RolePermissionId, cancellationToken);

        if (permission is null)
        {
            return Result.Failure(Error.NotFound(
                "RolePermission.NotFound",
                $"No permission found."));
        }

        repository.Delete(permission);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}