using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Concept.DeleteConcept
{
    public sealed record class DeleteConceptCommand(
        int PortfolioId,
        string Name
        ) : ICommand;
}

