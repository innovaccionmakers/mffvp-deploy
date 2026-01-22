using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Reports.Domain.LoadingInfo.Constants;

namespace Reports.Integrations.LoadingInfo.Commands;

[AuditLog]
public sealed record ProcessDailyDataCommand(
    int portfolioId,
    DateTime closingDateUtc,
    EtlSelection etlSelection
) : ICommand<ProcessDailyDataResponse>;
