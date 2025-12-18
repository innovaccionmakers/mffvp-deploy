using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Concept.GetConcepts
{
    [AuditLog]
    public sealed record GetConceptsQuery : IQuery<IReadOnlyCollection<GetConceptsResponse>>;
}

