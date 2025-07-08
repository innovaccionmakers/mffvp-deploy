using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.PreClosing.RunSimulation;
    public sealed record RunSimulationCommand(
        int PortfolioId,
        DateTime ClosingDate,
        bool IsClosing = false
    ) : ICommand<bool>;
