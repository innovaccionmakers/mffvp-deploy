namespace Reports.Domain.DailyClosing.Records;

public sealed record Rt1Record(Rt1Header Header, DateTime Date, int TotalRecords)
{
    public string ToLine()
    {
        var recordNumber = 1; // RT1 siempre es el primero
        const string area = "01";
        const string reportType = "17";
        var entityType = (Header.EntityType ?? string.Empty).PadLeft(2, '0');
        if (entityType.Length > 2)
        {
            entityType = entityType.Substring(0, 2);
        }
        return $"{recordNumber:00000}1{entityType}{Header.EntityCode}{FormatDate(Date)}{TotalRecords:00000}{Header.Keyword}{area}{reportType}";
    }

    private static string FormatDate(DateTime date)
        => date.ToString("ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture);
}
