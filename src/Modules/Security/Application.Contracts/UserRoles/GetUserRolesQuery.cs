using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.UserRoles;

public sealed record GetUserRolesQuery(int UserId) : IQuery<IReadOnlyCollection<UserRoleDto>>;
