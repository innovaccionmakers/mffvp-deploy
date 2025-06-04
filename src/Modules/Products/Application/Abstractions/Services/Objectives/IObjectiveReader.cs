using Products.Application.Objectives.GetObjectives;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.Application.Abstractions.Services.Objectives;

public interface IObjectiveReader
{
    Task<ObjectiveValidationContext> BuildValidationContextAsync(
        bool affiliateFound,
        int? affiliateId,
        StatusType requestedStatus,
        CancellationToken ct);

    Task<IReadOnlyList<ObjectiveDto>> ReadDtosAsync(
        int affiliateId,
        StatusType requestedStatus,
        CancellationToken ct);
}