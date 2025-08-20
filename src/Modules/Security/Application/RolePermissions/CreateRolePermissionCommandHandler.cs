using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.RolePermissions;
using Security.Domain.RolePermissions;
using Security.Domain.Roles;

using System.Data.Common;

namespace Security.Application.RolePermissions;

public sealed record CreateRolePermissionCommandHandler(
    IRolePermissionRepository rolePermissionRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRolePermissionCommand, int>
{
    public async Task<Result<int>> Handle(CreateRolePermissionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ScopePermission))
            return Result.Failure<int>(Error.Conflict(
                "Permission.Required",
                "The permission is required."));

        var roleExists = await roleRepository.ExistsAsync(request.RoleId, cancellationToken);
        if (!roleExists)
        {
            return Result.Failure<int>(Error.NotFound(
                "Role.NotFound",
                "The specified role does not exist."));
        }

        var exists = await rolePermissionRepository.ExistsAsync(request.RoleId, request.ScopePermission, cancellationToken);
        if (exists)
        {
            return Result.Failure<int>(Error.Conflict(
                "RolePermission.Exists",
                "The permission is already assigned to the specified role."));
        }

        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var result = RolePermission.Create(request.RoleId, request.ScopePermission);
        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        var permission = result.Value;

        rolePermissionRepository.Insert(permission);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(permission.Id);
    }
}
