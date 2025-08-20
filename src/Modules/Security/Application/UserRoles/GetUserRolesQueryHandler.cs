using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.UserRoles;
using Security.Domain.UserRoles;
using Security.Domain.Users;

namespace Security.Application.UserRoles;

public sealed class GetUserRolesQueryHandler(
    IUserRoleRepository userRoleRepository,
    IUserRepository userRepository)
    : IQueryHandler<GetUserRolesQuery, IReadOnlyCollection<UserRoleDto>>
{
    public async Task<Result<IReadOnlyCollection<UserRoleDto>>> Handle(
        GetUserRolesQuery request,
        CancellationToken cancellationToken)
    {
        var userExists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<IReadOnlyCollection<UserRoleDto>>(
                Error.NotFound("User.NotFound", $"User with ID {request.UserId} does not exist."));
        }

        var userRoles = await userRoleRepository.GetAllByUserIdAsync(request.UserId, cancellationToken);

        var result = userRoles
            .Select(ur => new UserRoleDto
            {
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name
            })
            .ToList();

        return Result.Success<IReadOnlyCollection<UserRoleDto>>(result);
    }
}