using Common.SharedKernel.Application.Reports;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public class ReportStrategyFactory : IReportStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ReportStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReportStrategy GetStrategy(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.Deposits => _serviceProvider.GetRequiredService<DepositsReportStrategy>(),
                ReportType.TechnicalSheet => _serviceProvider.GetRequiredService<TechnicalSheetStrategy>(),
                _ => throw new ArgumentException($"Tipo de reporte no soportado: {reportType}")
            };
        }
    }
}