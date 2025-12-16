using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConsecutivesSetup;

[AuditLog]
public sealed record UpdateConsecutiveSetupCommand(
    long Id,
    string SourceDocument,
    int Consecutive) : ICommand<ConsecutiveSetupResponse>;
