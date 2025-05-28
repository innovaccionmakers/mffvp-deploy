using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.GetActivateId;

public sealed record GetActivateIdQuery(
    string IdentificationType,
    string Identification) : IQuery<GetActivateIdResponse>;