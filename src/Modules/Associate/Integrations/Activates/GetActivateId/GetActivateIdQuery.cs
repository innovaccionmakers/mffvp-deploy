using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.GetActivateId;

public sealed record GetActivateIdQuery(
    string DocumentType,
    string Identification) : IQuery<GetActivateIdResponse>;