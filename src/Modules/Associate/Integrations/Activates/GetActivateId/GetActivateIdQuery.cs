using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.GetActivateId;

public sealed record GetActivateIdQuery(
    [property: HomologScope("TipoDocumento")]
    string DocumentType,
    string Identification) : IQuery<GetActivateIdResponse>;