using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConsecutivesSetup;

public sealed record UpdateConsecutiveSetupCommand(
    long Id,
    string Nature,
    string SourceDocument,
    int Consecutive) : ICommand<ConsecutiveSetupResponse>;
