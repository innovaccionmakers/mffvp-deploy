using Common.SharedKernel.Application.Messaging;

using Security.Domain.Roles;

namespace Security.Application.Contracts.Roles;

public sealed record class GetRolesQuery : IQuery<IReadOnlyCollection<Role>>;