using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.Users;
using Security.Domain.UserPermissions;

namespace Security.Application.Users;

public sealed class GetUserPermissionsQueryHandler(IUserPermissionRepository repository)
    : IQueryHandler<GetUserPermissionsQuery, IReadOnlyCollection<UserPermission>>
{
    public async Task<Result<IReadOnlyCollection<UserPermission>>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(cancellationToken);
        var filtered = result.Where(p => p.UserId == request.UserId).ToList();
        return Result.Success<IReadOnlyCollection<UserPermission>>(filtered);
    }
}
