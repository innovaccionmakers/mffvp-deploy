using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Domain.UserRoles;

namespace Security.Application.Contracts.UserRoles;

public sealed record GetUserRolesQuery(int UserId) : IQuery<IReadOnlyCollection<UserRoleDto>>;
