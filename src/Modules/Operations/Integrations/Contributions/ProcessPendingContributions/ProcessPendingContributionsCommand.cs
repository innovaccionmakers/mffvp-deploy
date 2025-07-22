using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.Contributions.ProcessPendingContributions;

public sealed record ProcessPendingContributionsCommand(int PortfolioId) : ICommand;

