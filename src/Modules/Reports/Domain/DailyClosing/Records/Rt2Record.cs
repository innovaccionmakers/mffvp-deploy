namespace Reports.Domain.TransmissionFormat.Records;

public sealed record Rt2Record(string PortfolioCode)
{
    public string ToLine(int recordNumber)
    {
        const string office = "0000";
        const string constant = "0";
        const string currency = "1";
        const string indicator = "1";
        const string trustType = "06"; //Por defecto Tipo Fondo 06
        var code = (PortfolioCode ?? string.Empty).PadLeft(6, '0');
        if (code.Length > 6)
        {
            code = code.Substring(0, 6);
        }
        return $"{recordNumber:00000000}2{office}{constant}{currency}{indicator}{trustType}{code}";
    }
}
