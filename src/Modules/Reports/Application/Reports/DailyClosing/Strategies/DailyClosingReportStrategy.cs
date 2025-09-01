using Common.SharedKernel.Application.Reports;
using Reports.Application.Reports.Common;
using Reports.Application.Reports.Common.Strategies;
using Reports.Domain.DailyClosing.Layout;
using Reports.Domain.DailyClosing.Records;

namespace Reports.Application.Reports.DailyClosing.Strategies;

public class DailyClosingReportStrategy : IReportGeneratorStrategy
{
    public ReportType ReportType => ReportType.DailyClosing;

    public int Generate(IReportData payload, IList<string> records, ref int recordNumber)
    {
        var data = (DailyReportData)payload;
        var generated = 0;

        // RT4 - 312
        recordNumber++;
        records.Add(Rt4Lines.UnitValue(data.UnitValue).ToLine(recordNumber));
        generated++;

        // RT4 - 313
        const string prefix = DailyClosingLayout.Rt4.R4313;

        recordNumber++;
        records.Add(Rt4Lines.PreviousUnits(data.PreviousUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.PreviousAmount(data.PreviousAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.YieldAmount(data.YieldAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.PreClosingAmount(data.PreClosingAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.ContributionUnits(data.ContributionUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.ContributionAmount(data.ContributionAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.TransferUnits(data.TransferUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.TransferAmount(data.TransferAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.PensionUnits(data.PensionUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.PensionAmount(data.PensionAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.WithdrawalUnits(data.WithdrawalUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.WithdrawalAmount(data.WithdrawalAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.OtherCommissionUnits(data.OtherCommissionUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.OtherCommissionAmount(data.OtherCommissionAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.VitalityTransferUnits(data.VitalityTransferUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.VitalityTransferAmount(data.VitalityTransferAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.OtherWithdrawalUnits(data.OtherWithdrawalUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.OtherWithdrawalAmount(data.OtherWithdrawalAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.CancellationUnits(data.CancellationUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.CancellationAmount(data.CancellationAmount).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.CurrentUnits(data.CurrentUnits).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.CurrentAmount(data.CurrentAmount).ToLine(recordNumber));
        generated++;
        // RT4 - 314 (signo siempre '+', magnitud absoluta)
        recordNumber++;
        records.Add(Rt4Lines.Return30Days(data.ReturnThirtyDay).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.Return180Days(data.ReturnOneHundredEightyDay).ToLine(recordNumber));
        generated++;

        recordNumber++;
        records.Add(Rt4Lines.Return365Days(data.ReturnThreeHundredSixtyFiveDay).ToLine(recordNumber));
        generated++;

        return generated;
    }
}
