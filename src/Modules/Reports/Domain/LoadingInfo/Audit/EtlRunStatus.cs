namespace Reports.Domain.LoadingInfo.Audit;

//resumen por ETL dentro de parametros
public static class EtlRunStatus
{
    public const string Pending = "pending";
    public const string Running = "running";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Skipped = "skipped";
}


