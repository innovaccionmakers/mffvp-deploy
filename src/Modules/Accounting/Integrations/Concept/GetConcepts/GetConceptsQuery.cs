using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Concept.GetConcepts
{
    public sealed record GetConceptsQuery : IQuery<IReadOnlyCollection<GetConceptsResponse>>;
}

