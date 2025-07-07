using Common.SharedKernel.Application.Messaging;
using Products.Domain.Alternatives;

namespace Products.Integrations.Alternatives;

public sealed record class GetAlternativesQuery : IQuery<IReadOnlyCollection<Alternative>>;
