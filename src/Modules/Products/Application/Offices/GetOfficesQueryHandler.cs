using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Offices;
using Products.Integrations.Offices;

namespace Products.Application.Offices;

internal sealed class GetOfficesQueryHandler(
    IOfficeRepository repository
) : IQueryHandler<GetOfficesQuery, IReadOnlyCollection<Office>>
{
    public async Task<Result<IReadOnlyCollection<Office>>> Handle(
        GetOfficesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetAllActiveAsync(cancellationToken);

        return Result.Success(result);
    }
}