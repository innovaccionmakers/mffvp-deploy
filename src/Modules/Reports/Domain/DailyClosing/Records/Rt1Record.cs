namespace Reports.Domain.TransmissionFormat.Records;

public sealed record Rt1Record(Rt1Header Header, DateTime Date, int TotalRecords)
{
    public string ToLine()
    {
        var recordNumber = 1;
        const string area = "01";
        const string reportType = "17";
        var entityType = (Header.EntityType ?? string.Empty).PadLeft(3, '0');
        if (entityType.Length > 3)
        {
            entityType = entityType.Substring(0, 3);
        }
        var entityCode = (Header.EntityCode ?? string.Empty).PadLeft(6, '0');
        if (entityCode.Length > 6)
        {
            entityCode = entityCode.Substring(0, 6);
        }
        return $"{recordNumber:00000000}1{entityType}{entityCode}{FormatDate(Date)}{TotalRecords:00000000}{Header.Keyword}{area}{reportType}";
    }

    private static string FormatDate(DateTime date)
        => date.ToString("ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture);
}
