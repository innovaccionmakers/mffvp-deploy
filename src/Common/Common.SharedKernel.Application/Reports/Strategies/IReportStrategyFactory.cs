using Common.SharedKernel.Application.Reports;

namespace Common.SharedKernel.Application.Reports.Strategies;

public interface IReportStrategyFactory
{
    IReportStrategy GetStrategy(ReportType reportType);
}