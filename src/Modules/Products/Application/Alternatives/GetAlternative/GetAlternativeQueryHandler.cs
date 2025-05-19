using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Integrations.Alternatives;
using Products.Integrations.Alternatives.GetAlternative;

namespace Products.Application.Alternatives.GetAlternative;

internal sealed class GetAlternativeQueryHandler(
    IAlternativeRepository alternativeRepository)
    : IQueryHandler<GetAlternativeQuery, AlternativeResponse>
{
    public async Task<Result<AlternativeResponse>> Handle(GetAlternativeQuery request,
        CancellationToken cancellationToken)
    {
        var alternative = await alternativeRepository.GetAsync(request.AlternativeId, cancellationToken);
        if (alternative is null)
            return Result.Failure<AlternativeResponse>(AlternativeErrors.NotFound(request.AlternativeId));
        var response = new AlternativeResponse(
            alternative.AlternativeId,
            alternative.AlternativeTypeId,
            alternative.Name,
            alternative.Status,
            alternative.Description
        );
        return response;
    }
}