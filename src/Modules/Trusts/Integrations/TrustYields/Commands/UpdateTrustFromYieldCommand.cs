using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Trusts.Domain.Trusts.TrustYield;

namespace Trusts.Integrations.TrustYields.Commands;

[AuditLog]
public sealed record UpdateTrustFromYieldCommand(
    IReadOnlyList<ApplyYieldRow> Rows,
    int BatchIndex
) : ICommand<ApplyYieldBulkBatchResult>;