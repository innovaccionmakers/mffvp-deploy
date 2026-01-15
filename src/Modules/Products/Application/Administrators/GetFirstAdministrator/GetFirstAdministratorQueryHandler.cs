using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Administrators;
using Products.Integrations.Administrators;
using Products.Integrations.Administrators.GetFirstAdministrator;

namespace Products.Application.Administrators.GetFirstAdministrator;

internal sealed class GetFirstAdministratorQueryHandler(
    IAdministratorRepository repository
) : IQueryHandler<GetFirstAdministratorQuery, AdministratorResponse?>
{
    public async Task<Result<AdministratorResponse?>> Handle(
        GetFirstAdministratorQuery request,
        CancellationToken cancellationToken)
    {
        var administrator = await repository.GetFirstOrderedByIdAsync(cancellationToken);

        if (administrator is null)
        {
            return Result.Success<AdministratorResponse?>(null);
        }

        var response = new AdministratorResponse(
            administrator.AdministratorId,
            administrator.Identification,
            administrator.IdentificationTypeId,
            administrator.Digit,
            administrator.Name,
            administrator.Status,
            administrator.EntityCode,
            administrator.EntityType,
            administrator.SfcEntityCode
        );

        return Result.Success<AdministratorResponse?>(response);
    }
}

