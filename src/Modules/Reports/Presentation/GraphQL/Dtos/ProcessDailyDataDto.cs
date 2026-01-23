

using Reports.Domain.LoadingInfo.Constants;
using Reports.Integrations.LoadingInfo.Commands;

namespace Reports.Presentation.GraphQL.Dtos;

public sealed record ProcessDailyDataDto(
    string ExecutionId,
    int PortfolioId,
    DateTime ClosingDate,
    string Status,
    EtlSelection Selection,
    IReadOnlyList<EtlStepResponse> Steps
);