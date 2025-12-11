using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain.Auth.Permissions;

namespace Security.Application.Contracts.Permissions;

public sealed record GetAllPermissionsQuery()
    : IQuery<IReadOnlyCollection<MakersPermissionBase>>;