
using Reports.Domain.LoadingInfo.Constants;


namespace Reports.Integrations.LoadingInfo.Commands;


public sealed record ProcessDailyDataResponse
{
    public string ExecutionId { get; init; }
    public int PortfolioId { get; init; }
    public DateTime ClosingDate { get; init; }
    public string Status { get; init; }
    public EtlSelection Selection { get; init; }
    public IReadOnlyList<EtlStepResponse> Steps { get; init; }

}
