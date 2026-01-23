namespace Reports.Domain.LoadingInfo.Audit;

public static class EtlPayloadLimits
{
    public const int MaxEtls = 20;

    public const int MaxKeyValues = 10;
    public const int MaxValueLength = 200;

    public const int MaxMetricsPerEtl = 5;
    public const int MaxWarningsPerEtl = 3;
}