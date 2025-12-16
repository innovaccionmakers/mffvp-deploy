using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Permissions;

public sealed record GetAllPermissionsQuery()
    : IQuery<IReadOnlyCollection<PermissionDtoBase>>;