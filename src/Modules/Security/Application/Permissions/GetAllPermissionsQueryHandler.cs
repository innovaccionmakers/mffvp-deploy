using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Auth.Permissions;

using Security.Application.Contracts.Permissions;

namespace Security.Application.Permissions;

public sealed class GetAllPermissionsQueryHandler
    : IQueryHandler<GetAllPermissionsQuery, IReadOnlyCollection<MakersPermissionBase>>
{
    public Task<Result<IReadOnlyCollection<MakersPermissionBase>>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var allPermissions = MakersPermissionsAccounting.All
            .Concat(MakersPermissionsAffiliates.All)
            .Concat(MakersPermissionsClosing.All)
            .Concat(MakersPermissionsCustomers.All)
            .Concat(MakersPermissionsOperations.All)
            .Concat(MakersPermissionsReports.All)
            .Concat(MakersPermissionsTreasury.All)
            .Cast<MakersPermissionBase>()
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyCollection<MakersPermissionBase>>(allPermissions));
    }
}