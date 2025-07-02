using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.Users;
using Security.Domain.UserRoles;

namespace Security.Application.Users;

public sealed class AssignRolesCommandHandler(IUserRoleRepository repository)
    : ICommandHandler<AssignRolesCommand>
{
    public async Task<Result> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        foreach (var rolePermissionId in request.RolePermissionIds)
        {
            var userRole = new UserRole(rolePermissionId, request.UserId);
            repository.Insert(userRole);
        }

        return Result.Success();
    }
}
