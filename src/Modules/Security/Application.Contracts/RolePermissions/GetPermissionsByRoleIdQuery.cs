using Common.SharedKernel.Application.Messaging;

using Security.Domain.RolePermissions;

namespace Security.Application.Contracts.RolePermissions;

public sealed record GetPermissionsByRoleIdQuery(int RoleId) : IQuery<IReadOnlyCollection<RolePermissionDto>>;
