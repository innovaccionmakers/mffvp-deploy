using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.Roles;
using Security.Domain.RolePermissions;

namespace Security.Application.Roles;

public sealed class AssignRolePermissionCommandHandler(IRolePermissionRepository repository)
    : ICommandHandler<AssignRolePermissionCommand>
{
    public async Task<Result> Handle(AssignRolePermissionCommand request, CancellationToken cancellationToken)
    {
        foreach (var scope in request.ScopePermissions)
        {
            var permission = new RolePermission(request.RoleId, scope);
            repository.Insert(permission);
        }

        return Result.Success();
    }
}
