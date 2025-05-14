using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.GetActivates;

public sealed record GetActivatesQuery : IQuery<IReadOnlyCollection<ActivateResponse>>;