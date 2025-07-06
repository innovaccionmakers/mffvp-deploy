using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.Users;
using Security.Domain.UserPermissions;

namespace Security.Application.Users;

public sealed class CreatePermissionsCommandHandler(IUserPermissionRepository repository)
    : ICommandHandler<CreatePermissionsCommand>
{
    public async Task<Result> Handle(CreatePermissionsCommand request, CancellationToken cancellationToken)
    {
        foreach (var (token, granted) in request.Permissions)
        {
            var userPermission = new UserPermission(request.UserId, token, granted);
            repository.Insert(userPermission);
        }

        return Result.Success();
    }
}