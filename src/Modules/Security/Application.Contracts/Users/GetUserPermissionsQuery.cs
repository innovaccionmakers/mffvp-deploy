using Common.SharedKernel.Application.Messaging;

using Security.Domain.UserPermissions;

namespace Security.Application.Contracts.Users;

public sealed record class GetUserPermissionsQuery(int UserId) : IQuery<IReadOnlyCollection<UserPermission>>;