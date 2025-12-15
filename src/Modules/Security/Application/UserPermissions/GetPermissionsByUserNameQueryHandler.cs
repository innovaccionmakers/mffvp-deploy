using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using MediatR;

using Security.Application.Contracts.Permissions;
using Security.Application.Contracts.RolePermissions;
using Security.Application.Contracts.UserPermissions;
using Security.Domain.RolePermissions;
using Security.Domain.UserPermissions;
using Security.Domain.UserRoles;
using Security.Domain.Users;

namespace Security.Application.UserPermissions;

public sealed class GetPermissionsByUserNameQueryHandler(
    IUserRepository userRepository,
    IUserPermissionRepository userPermissionRepository,
    IUserRoleRepository userRoleRepository,
    IRolePermissionRepository rolePermissionRepository,
    ISender sender)
    : IQueryHandler<GetPermissionsByUserNameQuery, IReadOnlyCollection<PermissionDtoBase>>
{
    public async Task<Result<IReadOnlyCollection<PermissionDtoBase>>> Handle(GetPermissionsByUserNameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result.Failure<IReadOnlyCollection<PermissionDtoBase>>(Error.Validation(
                "User.UserName.Required",
                "The username is required."));
        }

        var user = await userRepository.GetByUserNameAsync(request.UserName);
        if (user is null)
        {
            return Result.Failure<IReadOnlyCollection<PermissionDtoBase>>(Error.NotFound(
                "User.NotFound",
                "The user was not found."));
        }

        var allPermissionsResult = await sender.Send(new GetAllPermissionsQuery(), cancellationToken);
        if (allPermissionsResult.IsFailure)
            return Result.Failure<IReadOnlyCollection<PermissionDtoBase>>(allPermissionsResult.Error);

        var allDefinedPermissions = allPermissionsResult.Value;

        var userPermissions = await userPermissionRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var roleIds = await userRoleRepository.GetRoleIdsByUserIdAsync(user.Id);
        var permissionsFromRoles = await rolePermissionRepository.GetPermissionsByRoleIdsAsync(roleIds);

        var grantedUserPermissions = userPermissions
            .Where(up => up.Granted)
            .Select(up => up.PermitToken.ToString());

        var combined = grantedUserPermissions
            .Concat(permissionsFromRoles)
            .Distinct()
            .Select(scope => allDefinedPermissions.FirstOrDefault(p => p.ScopePermission == scope))
            .Where(p => p is not null)
            .Cast<PermissionDtoBase>()
            .ToList();

        return Result.Success<IReadOnlyCollection<PermissionDtoBase>>(combined);
    }
}