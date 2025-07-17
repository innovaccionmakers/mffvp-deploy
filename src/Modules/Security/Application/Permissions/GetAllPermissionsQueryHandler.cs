using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Auth.Permissions;

using Security.Application.Contracts.Permissions;

namespace Security.Application.Permissions;

public sealed class GetAllPermissionsQueryHandler
    : IQueryHandler<GetAllPermissionsQuery, IReadOnlyCollection<MakersPermission>>
{
    public Task<Result<IReadOnlyCollection<MakersPermission>>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var allPermissions = MakersPermissionsAssociateActivates.All
            .Concat(MakersPermissionsAssociatePensionRequirements.All)
            .Concat(MakersPermissionsOperationsContributionTx.All)
            .Concat(MakersPermissionsAccounting.All)
            .Concat(MakersPermissionsAffiliates.All)
            .Concat(MakersPermissionsClosing.All)
            .Concat(MakersPermissionsOperations.All)
            .Concat(MakersPermissionsReports.All)
            .Concat(MakersPermissionsTreasury.All)
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyCollection<MakersPermission>>(allPermissions));
    }
}