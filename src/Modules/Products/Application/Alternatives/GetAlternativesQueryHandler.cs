using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Integrations.Alternatives;

namespace Products.Application.Alternatives;

public class GetAlternativesQueryHandler(
    IAlternativeRepository repository) : IQueryHandler<GetAlternativesQuery, IReadOnlyCollection<Alternative>>
{
    public async Task<Result<IReadOnlyCollection<Alternative>>> Handle(
        GetAlternativesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(cancellationToken);

        return Result.Success(result);
    }
}
