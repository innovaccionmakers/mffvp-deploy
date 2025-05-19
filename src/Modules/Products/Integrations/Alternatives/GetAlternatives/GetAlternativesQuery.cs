using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Alternatives.GetAlternatives;

public sealed record GetAlternativesQuery : IQuery<IReadOnlyCollection<AlternativeResponse>>;