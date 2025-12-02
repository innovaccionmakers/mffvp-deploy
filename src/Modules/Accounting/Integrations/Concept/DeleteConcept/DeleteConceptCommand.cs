using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Concept.DeleteConcept
{
    public sealed record class DeleteConceptCommand(
        long ConceptId
        ) : ICommand;
}

