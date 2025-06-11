using Common.SharedKernel.Application.Attributes;

namespace Associate.Integrations.Activates.GetActivateId;

public sealed record GetActivateIdResponse(
    int ActivateId,
    [property: HomologScope("TipoDocumento")]
    Guid IdentificationType,
    bool Pensioner
    );
