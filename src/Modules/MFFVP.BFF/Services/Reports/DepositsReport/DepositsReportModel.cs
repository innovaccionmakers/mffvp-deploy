using MFFVP.BFF.Services.Reports.Models;

namespace MFFVP.BFF.Services.Reports.DepositsReport
{
    public class DepositsReportModel : ReportModelBase
    {
        public string AccountType { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal TransactionValue { get; set; }
        public string CheckNumber { get; set; }
        public string Nature { get; set; }
        public string Observations { get; set; }
        public string TransactionName { get; set; }
        public string AdditionalInfo { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public string Reference3 { get; set; }
        public int Branch { get; set; }

        public override object[] ToRowData()
        {
            return new object[]
            {
                AccountType,
                AccountNumber,
                TransactionCode,
                EffectiveDate.ToString("yyyyMMdd"),
                Math.Round(TransactionValue, 2),
                CheckNumber ?? string.Empty,
                Nature,
                Observations ?? string.Empty,
                TransactionName ?? string.Empty,
                AdditionalInfo ?? string.Empty,
                Reference1 ?? string.Empty,
                Reference2 ?? string.Empty,
                Reference3 ?? string.Empty,
                Branch
            };
        }
    }
}
