using System.Text;
using Common.SharedKernel.Application.Reports;
using Reports.Application.Reports.Common.Strategies;
using Reports.Domain.TransmissionFormat;
using Reports.Domain.TransmissionFormat.Records;

namespace Reports.Application.Reports.TransmissionFormat;

public interface ITransmissionFormatReportBuilder
{
    string Build(Rt1Header rt1Header, IEnumerable<(Rt2Header header, TransmissionFormatReportData data)> portfolioData, DateTime generationDate);
}

public class TransmissionFormatReportBuilder(IReportStrategyBuilder strategyBuilder) : ITransmissionFormatReportBuilder
{
    public string Build(Rt1Header rt1Header, IEnumerable<(Rt2Header header, TransmissionFormatReportData data)> portfolioData, DateTime generationDate)
    {
        var records = new List<string>();
        var recordNumber = 1;

        var strategy = strategyBuilder.Build(ReportType.TransmissionFormat);

        foreach (var (header, data) in portfolioData)
        {
            recordNumber++;
            var rt2 = new Rt2Record(header.PortfolioCode).ToLine(recordNumber);
            records.Add(rt2);

            strategy.Generate(data, records, ref recordNumber);
        }

        recordNumber++;
        var rt5 = new Rt5Record().ToLine(recordNumber);
        records.Add(rt5);

        var rt1 = new Rt1Record(rt1Header, generationDate, recordNumber).ToLine();
        records.Insert(0, rt1);

        var builder = new StringBuilder();
        foreach (var line in records)
        {
            builder.AppendLine(line);
        }

        return builder.ToString();
    }
}
