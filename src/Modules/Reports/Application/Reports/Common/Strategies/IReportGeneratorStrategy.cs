using Common.SharedKernel.Application.Reports;

namespace Reports.Application.Reports.Common.Strategies;

public interface IReportGeneratorStrategy
{
    ReportType ReportType { get; }
    int Generate(IReportData data, IList<string> records, ref int recordNumber);
}

