using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.RolePermissions;

public sealed record GetPermissionsByRoleIdQuery(int RoleId) : IQuery<IReadOnlyCollection<RolePermissionDto>>;
