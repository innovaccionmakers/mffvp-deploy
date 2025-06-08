namespace Operations.Application.Contributions.CreateContribution;

internal static class ConfigurationParameterUuids
{
    public static readonly Guid TaxExempt        = Guid.Parse("d208f567-0000-0000-0000-000000000001");
    public static readonly Guid TaxNoRetention   = Guid.Parse("d208f567-0000-0000-0000-000000000002");
    public static readonly Guid TaxRetention     = Guid.Parse("d208f567-0000-0000-0000-000000000003");
    public static readonly Guid RetentionPct     = Guid.Parse("d208f567-0000-0000-0000-000000000004");
    public static readonly Guid CertifiedState   = Guid.Parse("d208f567-0000-0000-0000-000000000005");
    public static readonly Guid UncertifiedState = Guid.Parse("d208f567-0000-0000-0000-000000000006");
}