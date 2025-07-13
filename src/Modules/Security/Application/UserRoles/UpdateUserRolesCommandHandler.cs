using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.UserRoles;
using Security.Domain.UserRoles;

using System.Data.Common;

namespace Security.Application.UserRoles;

public sealed record UpdateUserRolesCommandHandler(
        IUserRoleRepository repository,
        IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserRolesCommand>
{
    public async Task<Result> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        if (request.RolePermissionsIds == null || request.RolePermissionsIds.Count == 0)
        {
            return Result.Failure(Error.Validation(
                "UserRole.EmptyList",
                "At least one RolePermissionsId must be provided."));
        }

        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var currentUserRoles = await repository.GetAllByUserIdAsync(request.UserId, cancellationToken);
        var currentRoleIds = currentUserRoles.Select(x => x.RoleId).ToHashSet();
        var incomingRoleIds = request.RolePermissionsIds.ToHashSet();

        var rolesToAdd = incomingRoleIds.Except(currentRoleIds);
        foreach (var rolePermissionId in rolesToAdd)
        {
            var result = UserRole.Create(rolePermissionId, request.UserId);
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
