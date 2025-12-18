using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Concept.DeleteConcept
{
    [AuditLog]
    public sealed record class DeleteConceptCommand(
        long ConceptId
        ) : ICommand;
}

