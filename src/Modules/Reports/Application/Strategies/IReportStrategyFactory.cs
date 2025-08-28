using Common.SharedKernel.Application.Reports;

namespace Reports.Application.Strategies
{
    public interface IReportStrategyFactory
    {
        IReportStrategy GetStrategy(ReportType reportType);
    }
}