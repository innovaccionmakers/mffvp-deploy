using Common.SharedKernel.Application.Reports;

namespace Reports.Application.Reports.Common.Strategies;

public interface IReportStrategyBuilder
{
    IReportGeneratorStrategy Build(ReportType reportType);
}

public class ReportStrategyBuilder : IReportStrategyBuilder
{
    private readonly IEnumerable<IReportGeneratorStrategy> strategies;

    public ReportStrategyBuilder(IEnumerable<IReportGeneratorStrategy> strategies)
    {
        this.strategies = strategies;
    }

    public IReportGeneratorStrategy Build(ReportType reportType)
    {
        var strategy = strategies.FirstOrDefault(x => x.ReportType == reportType);

        return strategy ?? throw new InvalidOperationException($"Strategy not found for {reportType}");
    }
}

