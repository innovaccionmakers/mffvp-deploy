using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.TreasuryConcepts;

namespace Treasury.Integrations.TreasuryConcepts.GetTreasuryConcepts;

public sealed record GetTreasuryConceptsQuery() : IQuery<IReadOnlyCollection<TreasuryConcept>>;
