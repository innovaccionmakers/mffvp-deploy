using Common.SharedKernel.Application.Messaging;

using Security.Domain.RolePermissions;

namespace Security.Application.Contracts.Roles;

public sealed record class GetRolePermissionsQuery(int RoleId) : IQuery<IReadOnlyCollection<RolePermission>>;
