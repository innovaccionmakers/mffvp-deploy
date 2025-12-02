using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConsecutivesSetup;

public sealed record GetConsecutivesSetupQuery() : IQuery<IReadOnlyCollection<ConsecutiveSetupResponse>>;
