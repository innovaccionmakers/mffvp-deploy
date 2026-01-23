
using Reports.Domain.LoadingInfo.Constants;

namespace Reports.Presentation.GraphQL.Dtos;

public sealed record ProcessDailyDataInput(
    int PortfolioId,
    DateTime ClosingDateUtc,
    EtlSelection EtlSelection
);