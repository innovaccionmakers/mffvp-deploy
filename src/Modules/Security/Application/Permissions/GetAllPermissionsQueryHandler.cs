using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Auth.Permissions;

using Security.Application.Contracts.Permissions;

namespace Security.Application.Permissions;

public sealed class GetAllPermissionsQueryHandler
    : IQueryHandler<GetAllPermissionsQuery, IReadOnlyCollection<PermissionDtoBase>>
{
    public Task<Result<IReadOnlyCollection<PermissionDtoBase>>> Handle(
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
            .Select(PermissionDtoFactory.CreateFrom)
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyCollection<PermissionDtoBase>>(allPermissions));
    }
}