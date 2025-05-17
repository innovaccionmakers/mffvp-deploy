using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Integrations.Alternatives;
using Products.Integrations.Alternatives.GetAlternatives;

namespace Products.Application.Alternatives.GetAlternatives;

internal sealed class GetAlternativesQueryHandler(
    IAlternativeRepository alternativeRepository)
    : IQueryHandler<GetAlternativesQuery, IReadOnlyCollection<AlternativeResponse>>
{
    public async Task<Result<IReadOnlyCollection<AlternativeResponse>>> Handle(GetAlternativesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await alternativeRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new AlternativeResponse(
                e.AlternativeId,
                e.AlternativeTypeId,
                e.Name,
                e.Status,
                e.Description))
            .ToList();

        return Result.Success<IReadOnlyCollection<AlternativeResponse>>(response);
    }
}