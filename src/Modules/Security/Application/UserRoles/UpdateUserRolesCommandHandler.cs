using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.UserRoles;
using Security.Domain.Roles;
using Security.Domain.UserRoles;
using Security.Domain.Users;

using System.Data.Common;

namespace Security.Application.UserRoles;

public sealed record UpdateUserRolesCommandHandler(
        IUserRoleRepository repository,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserRolesCommand>
{
    public async Task<Result> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        if (request.RoleIds == null || request.RoleIds.Count == 0)
        {
            return Result.Failure(Error.Validation(
                "UserRole.EmptyList",
                "At least one RolePermissionsId must be provided."));
        }

        var userExists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure(Error.NotFound("User.NotFound", $"User with ID {request.UserId} does not exist."));
        }

        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var currentUserRoles = await repository.GetAllByUserIdAsync(request.UserId, cancellationToken);
        var currentRoleIds = currentUserRoles.Select(x => x.RoleId).ToHashSet();
        var incomingRoleIds = request.RoleIds.ToHashSet();

        var rolesToAdd = incomingRoleIds.Except(currentRoleIds);
        foreach (var roleId in rolesToAdd)
        {
            var roleExists = await roleRepository.ExistsAsync(roleId, cancellationToken);
            if (!roleExists)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure(Error.NotFound("Role.NotFound", $"Role with ID {roleId} does not exist."));
            }

            var result = UserRole.Create(roleId, request.UserId);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure(result.Error);
            }

            repository.Insert(result.Value);
        }

        var toRemove = currentUserRoles.Where(x => !incomingRoleIds.Contains(x.RoleId)).ToList();
        foreach (var obsolete in toRemove)
        {
            repository.Delete(obsolete);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("User roles successfully synchronized.");
    }
}
