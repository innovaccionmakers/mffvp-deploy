using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.RolePermissions;
using Security.Domain.RolePermissions;

using System.Data.Common;

namespace Security.Application.RolePermissions;

public sealed record CreateRolePermissionCommandHandler(
        IRolePermissionRepository repository,
        IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRolePermissionCommand>
{
    public async Task<Result> Handle(CreateRolePermissionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ScopePermission))
            return Result.Failure(Error.Conflict(
                "Permission.Required",
                "The permission is required."));

        var exists = await repository.ExistsAsync(request.RoleId, request.ScopePermission, cancellationToken);

        if (exists)
        {
            return Result.Failure(Error.Conflict(
                "RolePermission.Exists",
                "The permission is already assigned to the specified role."));
        }

        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var result = RolePermission.Create(request.RoleId, request.ScopePermission);

        var permission = result.Value;

        repository.Insert(permission);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}

