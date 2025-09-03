namespace Reports.Domain.DailyClosing.Records;

public sealed record Rt5Record()
{
    public string ToLine(int recordNumber)
    {
        return $"{recordNumber:00000}5";
    }
}

