using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConsecutivesSetup;

[AuditLog]
public sealed record GetConsecutivesSetupQuery() : IQuery<IReadOnlyCollection<ConsecutiveSetupResponse>>;
