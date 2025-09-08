namespace Reports.Domain.DailyClosing.Records;

public sealed record Rt2Record(string PortfolioCode)
{
    public string ToLine(int recordNumber)
    {
        const string office = "0000";
        const string constant = "0";
        const string currency = "1";
        const string indicator = "1";
        const string trustType = "6";
        var code = (PortfolioCode ?? string.Empty).PadLeft(4, '0');
        if (code.Length > 4)
        {
            code = code.Substring(0, 4);
        }
        return $"{recordNumber:00000}2{office}{constant}{currency}{indicator}{trustType}{code}";
    }
}
