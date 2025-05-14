using Associate.Domain.Activates;
using Associate.Integrations.Activates;
using Associate.Integrations.Activates.GetActivates;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Application.Activates.GetActivates;

internal sealed class GetActivatesQueryHandler(
    IActivateRepository activateRepository)
    : IQueryHandler<GetActivatesQuery, IReadOnlyCollection<ActivateResponse>>
{
    public async Task<Result<IReadOnlyCollection<ActivateResponse>>> Handle(GetActivatesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await activateRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new ActivateResponse(
                e.ActivateId,
                e.IdentificationType,
                e.Identification,
                e.Pensioner,
                e.MeetsPensionRequirements,
                e.ActivateDate))
            .ToList();

        return Result.Success<IReadOnlyCollection<ActivateResponse>>(response);
    }
}