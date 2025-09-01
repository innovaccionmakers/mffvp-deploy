using Common.SharedKernel.Application.Reports;

namespace Reports.Application.Reports.Strategies
{
    public interface IReportStrategyFactory
    {
        IReportStrategy GetStrategy(ReportType reportType);
    }
}