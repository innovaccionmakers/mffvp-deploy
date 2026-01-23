namespace Accounting.Integrations.ConfigurationGenerals.GetConfigurationGenerals
{
    public sealed record class GetConfigurationGeneralsResponse(
            long Id,
            string AccountingCode,
            string CostCenter
        );
}
