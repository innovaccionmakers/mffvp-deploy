using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.GetActivate;

public sealed record GetActivateQuery(long ActivateId) : IQuery<ActivateResponse>;