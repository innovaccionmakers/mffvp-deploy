using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.Users;
using Security.Domain.Roles;
using Security.Domain.UserRoles;
using Security.Domain.Users;

namespace Security.Application.Users;

public sealed class CreateUserWithRolesCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserWithRolesCommand, int>
{
    public async Task<Result<int>> Handle(CreateUserWithRolesCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return Result.Failure<int>(Error.Validation("User.UserName.Required", "The username is required."));

        if (request.Roles is null || request.Roles.Count == 0)
            return Result.Failure<int>(Error.Validation("UserRole.EmptyList", "At least one RolePermissionsId must be provided."));

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var userResult = User.Create(
            request.Id,
            request.UserName,
            request.Name,
            request.MiddleName,
            request.Identification,
            request.Email,
            request.DisplayName
        );

        if (userResult.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<int>(userResult.Error);
        }

        var user = userResult.Value;
        userRepository.Insert(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var roleReq in request.Roles)
        {
            if (roleReq.Id <= 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<int>(Error.Validation("Role.Id.Invalid", "The role ID must be greater than zero."));
            }

            if (string.IsNullOrWhiteSpace(roleReq.Name))
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<int>(Error.Validation("Role.Name.Required", "The role name is required."));
            }

            var exists = await roleRepository.ExistsAsync(roleReq.Id, cancellationToken);
            if (!exists)
            {
                var createRoleResult = Role.Create(roleReq.Id, roleReq.Name, roleReq.Objective);
                if (createRoleResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure<int>(createRoleResult.Error);
                }

                roleRepository.Insert(createRoleResult.Value);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var currentUserRoles = await userRoleRepository.GetAllByUserIdAsync(user.Id, cancellationToken);
        var currentRoleIds = currentUserRoles.Select(x => x.RoleId).ToHashSet();
        var incomingRoleIds = request.Roles.Select(r => r.Id).ToHashSet();

        var rolesToAdd = incomingRoleIds.Except(currentRoleIds);
        foreach (var roleId in rolesToAdd)
        {
            var roleExists = await roleRepository.ExistsAsync(roleId, cancellationToken);
            if (!roleExists)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<int>(Error.NotFound("Role.NotFound", $"Role with ID {roleId} does not exist."));
            }

            var linkResult = UserRole.Create(roleId, user.Id);
            if (linkResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<int>(linkResult.Error);
            }

            userRoleRepository.Insert(linkResult.Value);
        }

        var toRemove = currentUserRoles.Where(x => !incomingRoleIds.Contains(x.RoleId)).ToList();
        foreach (var obsolete in toRemove)
            userRoleRepository.Delete(obsolete);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
