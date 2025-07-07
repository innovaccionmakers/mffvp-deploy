using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Commercials;
using Products.Integrations.Commercials;

namespace Products.Application.Commercials;

internal sealed class GetCommercialsQueryHandler(
    ICommercialRepository repository
) : IQueryHandler<GetCommercialsQuery, IReadOnlyCollection<Commercial>>
{
    public async Task<Result<IReadOnlyCollection<Commercial>>> Handle(
        GetCommercialsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetAllActiveAsync(cancellationToken);

        return Result.Success(result);
    }
}
