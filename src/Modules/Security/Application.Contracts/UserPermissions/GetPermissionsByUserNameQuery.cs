using Common.SharedKernel.Application.Messaging;

using Security.Application.Contracts.Permissions;

namespace Security.Application.Contracts.UserPermissions;

public sealed record GetPermissionsByUserNameQuery(string UserName)
    : IQuery<IReadOnlyCollection<PermissionDto>>;